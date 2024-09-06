namespace TerraUtil.BuildSystem.Core;

public class FileEntry(string name, int offset, int length, int compressedLength, byte[] cachedBytes)
{
    public string Name { get; } = name;

    // from the start of the file
    public int Offset { get; internal set; } = offset;
    public int Length { get; } = length;
    public int CompressedLength { get; } = compressedLength;

    // intended to be readonly, but unfortunately no ReadOnlySpan on .NET 4.5
    internal byte[] cachedBytes = cachedBytes;
}
