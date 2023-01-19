using System.Buffers.Binary;
using System.IO.Compression;

namespace KSharpPlus.Net.WebSocket; 

internal sealed class PayloadDecompressor : IDisposable {
    const uint ZlibFlush = 0x0000FFFF;
    const byte ZlibPrefix = 0x78;

    MemoryStream CompressedStream { get; }
    DeflateStream? DecompressorStream { get; }
    
    public PayloadDecompressor() {
        CompressedStream = new MemoryStream();
        DecompressorStream = new DeflateStream(CompressedStream, CompressionMode.Decompress);
    }

    public bool TryDecompress(ArraySegment<byte> compressed, MemoryStream decompressed) {
        DeflateStream zlib = DecompressorStream!;

        if (compressed.Array?[0] == ZlibPrefix)
            CompressedStream.Write(compressed.Array, compressed.Offset + 2, compressed.Count - 2);
        else
            CompressedStream.Write(compressed.Array!, compressed.Offset, compressed.Count);

        CompressedStream.Flush();
        CompressedStream.Position = 0;

        Span<byte> compressedSpan = compressed.AsSpan();
        uint suffix = BinaryPrimitives.ReadUInt32BigEndian(compressedSpan[^4..]);
        if (suffix != ZlibFlush) return false;

        try {
            zlib.CopyTo(decompressed);
            return true;
        } catch {
            return false;
        } finally {
            CompressedStream.Position = 0;
            CompressedStream.SetLength(0);
        }
    }
    
    public void Dispose() {
        DecompressorStream?.Dispose();
        CompressedStream.Dispose();
    }
}