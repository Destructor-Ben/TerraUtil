// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TerraUtil.BuildSystem.Core;

public class FileEntry(string name, int offset, int length, int compressedLength, byte[] data)
{
    public string Name { get; } = name;
    public int Offset { get; set; } = offset; // Offset from the start of the file table in the tmod file
    public int Length { get; } = length;
    public int CompressedLength { get; } = compressedLength;
    public byte[] Data { get; } = data;
}
