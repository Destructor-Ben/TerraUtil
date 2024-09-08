using System.Collections.Concurrent;
using System.IO.Compression;
using System.Security.Cryptography;

namespace TerraUtil.BuildSystem.Core;

public class ModFile(string path, string name, Version version, Version modLoaderVersion)
{
    private const uint MinCompressSize = 1 << 10; // 1KB
    private const float CompressionTradeoff = 0.9f;

    private string Path { get; } = path;
    private string Name { get; } = name;
    private Version Version { get; } = version;
    private Version ModLoaderVersion { get; } = modLoaderVersion;
    private ConcurrentBag<FileEntry> Files { get; } = [];

    private static string Sanitize(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// Adds a file entry to the mod file.
    /// This method is not thread safe with reads, but is thread safe with multiple concurrent AddFile calls.
    /// </summary>
    /// <param name="fileName">The internal filepath, which will be slash sanitised automatically.</param>
    /// <param name="data">The file content to add.
    /// <para />
    /// WARNING: Data is kept as a shallow copy, so modifications to the passed byte array will affect the files content.
    /// </param>
    public void AddFile(string fileName, byte[] data)
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

        Files.Add(new FileEntry(fileName, -1, size, data.Length, data));
    }

    public void Save()
    {
        Save(Path);
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        using var fileStream = File.Create(path);
        Save(fileStream);
    }

    public void Save(Stream stream)
    {
        // TMOD magic number (identifier)
        // tModLoader version
        // Hash
        // Signature
        // Data length
        // Signed data

        using var writer = new BinaryWriter(stream);

        writer.Write("TMOD"u8.ToArray());
        writer.Write(ModLoaderVersion.ToString());

        int hashPos = (int)stream.Position;
        writer.Write(new byte[20 + 256 + 4]); // Hash, sig, data length

        int dataPos = (int)stream.Position;
        writer.Write(Name);
        writer.Write(Version.ToString());

        // File count
        // File entries:
        //   Filename
        //   Uncompressed file size
        //   Compressed file size (stored size)

        writer.Write(Files.Count);
        foreach (var f in Files)
        {
            if (f.CompressedLength != f.Data.Length)
                throw new Exception($"CompressedLength ({f.CompressedLength}) != Data.Length ({f.Data.Length}): {f.Name}");

            writer.Write(f.Name);
            writer.Write(f.Length);
            writer.Write(f.CompressedLength);
        }

        // Write compressed files and update offsets
        int offset = (int)stream.Position; // Offset starts at end of file table
        foreach (var f in Files)
        {
            writer.Write(f.Data);

            f.Offset = offset;
            offset += f.CompressedLength;
        }

        // Update hash
        stream.Position = dataPos;
        byte[] hash = SHA1.HashData(stream);

        stream.Position = hashPos;
        writer.Write(hash);

        // Skip signature
        stream.Seek(256, SeekOrigin.Current);

        // Write data length
        writer.Write((int)(stream.Length - dataPos));
    }

    // Ignore file extensions which don't compress well under deflate to improve build time
    private static bool ShouldCompress(string fileName)
    {
        string fileExtension = System.IO.Path.GetExtension(fileName);
        return fileExtension != ".png" && fileExtension != ".mp3" && fileExtension != ".ogg";
    }
}
