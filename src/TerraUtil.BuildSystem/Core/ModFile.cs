using System.Collections.Concurrent;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace TerraUtil.BuildSystem.Core;

public class ModFile
{
    private const uint MinCompressSize = 1 << 10; //1KB
    private const float CompressionTradeoff = 0.9f;

    private static string Sanitize(string path) => path.Replace('\\', '/');

    private readonly string _path;

    private readonly ConcurrentBag<FileEntry> _files = new();

    private Version ModLoaderVersion { get; }

    private string Name { get; }

    private Version Version { get; }

    internal ModFile(string path, string name, Version version, Version modLoaderVersion)
    {
        _path = path;
        Name = name;
        Version = version;
        ModLoaderVersion = modLoaderVersion;
    }

    /// <summary>
    /// Adds a (fileName -> content) entry to the compressed payload
    /// This method is not threadsafe with reads, but is threadsafe with multiple concurrent AddFile calls
    /// </summary>
    /// <param name="fileName">The internal filepath, will be slash sanitised automatically</param>
    /// <param name="data">The file content to add. WARNING, data is kept as a shallow copy, so modifications to the passed byte array will affect file content</param>
    internal void AddFile(string fileName, byte[] data)
    {
        fileName = Sanitize(fileName);
        int size = data.Length;

        if (size > MinCompressSize && ShouldCompress(fileName))
        {
            using var ms = new MemoryStream(data.Length);
            using (var ds = new DeflateStream(ms, CompressionMode.Compress))
            {
                ds.Write(data, 0, data.Length);
            }

            byte[] compressed = ms.ToArray();
            if (compressed.Length < size * CompressionTradeoff)
                data = compressed;
        }

        _files.Add(new FileEntry(fileName, -1, size, data.Length, data));
    }

    public void Save()
    {
        Save(_path);
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var fileStream = File.Create(path);
        Save(fileStream);
    }

    public void Save(Stream stream)
    {
        // write the general TMOD header and data blob
        // TMOD ascii identifier
        // tModLoader version
        // hash
        // signature
        // data length
        // signed data
        using var writer = new BinaryWriter(stream);

        writer.Write(Encoding.ASCII.GetBytes("TMOD"));
        writer.Write(ModLoaderVersion.ToString());

        int hashPos = (int)stream.Position;
        writer.Write(new byte[20 + 256 + 4]); //hash, sig, data length

        int dataPos = (int)stream.Position;
        writer.Write(Name);
        writer.Write(Version.ToString());

        // write file table
        // file count
        // file-entries:
        //   filename
        //   uncompressed file size
        //   compressed file size (stored size)
        writer.Write(_files.Count);

        foreach (var f in _files)
        {
            if (f.CompressedLength != f.cachedBytes.Length)
                throw new Exception($"CompressedLength ({f.CompressedLength}) != cachedBytes.Length ({f.cachedBytes.Length}): {f.Name}");

            writer.Write(f.Name);
            writer.Write(f.Length);
            writer.Write(f.CompressedLength);
        }

        // write compressed files and update offsets
        int offset = (int)stream.Position; // offset starts at end of file table
        foreach (var f in _files)
        {
            writer.Write(f.cachedBytes);

            f.Offset = offset;
            offset += f.CompressedLength;
        }

        // update hash
        stream.Position = dataPos;
        byte[] hash = SHA1.Create().ComputeHash(stream);

        stream.Position = hashPos;
        writer.Write(hash);

        //skip signature
        stream.Seek(256, SeekOrigin.Current);

        // write data length
        writer.Write((int)(stream.Length - dataPos));
    }

    // Ignore file extensions which don't compress well under deflate to improve build time
    private static bool ShouldCompress(string fileName) => !fileName.EndsWith(".png") && !fileName.EndsWith(".mp3") && !fileName.EndsWith(".ogg");
}
