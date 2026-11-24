using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Mkx.Templates.Server.Services;

/// <summary>Small portable PNG renderer; uses only the .NET runtime, including on Linux.</summary>
internal static class LoginCaptchaImage
{
    private const int Width = 180;
    private const int Height = 56;
    private const int Stride = 1 + Width * 3;

    // Seven rows of five pixels for digits 2–9.
    private static readonly byte[][] Glyphs =
    [
        [14, 17, 1, 2, 4, 8, 31],
        [30, 1, 1, 14, 1, 1, 30],
        [2, 6, 10, 18, 31, 2, 2],
        [31, 16, 16, 30, 1, 17, 14],
        [6, 8, 16, 30, 17, 17, 14],
        [31, 1, 2, 4, 8, 8, 8],
        [14, 17, 17, 14, 17, 17, 14],
        [14, 17, 17, 15, 1, 2, 12]
    ];

    public static string Render(string answer)
    {
        var pixels = new byte[Stride * Height];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            SetPixel(pixels, x, y, 237, 243, 250);

        for (var i = 0; i < 14; i++)
        {
            var x1 = RandomNumberGenerator.GetInt32(Width);
            var x2 = RandomNumberGenerator.GetInt32(Width);
            var y1 = RandomNumberGenerator.GetInt32(Height);
            var y2 = RandomNumberGenerator.GetInt32(Height);
            var steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1)) + 1;
            for (var j = 0; j <= steps; j++)
                SetPixel(pixels, x1 + (x2 - x1) * j / steps, y1 + (y2 - y1) * j / steps, 171, 196, 220);
        }

        for (var i = 0; i < answer.Length; i++)
        {
            var glyph = Glyphs[answer[i] - '2'];
            var angle = RandomNumberGenerator.GetInt32(-13, 14) * Math.PI / 180;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var centerX = 25 + i * 32 + RandomNumberGenerator.GetInt32(-2, 3);
            var centerY = 28 + RandomNumberGenerator.GetInt32(-3, 4);
            const int scale = 5;
            // Inverse map each destination pixel to a rotated glyph to avoid holes.
            for (var y = centerY - 25; y <= centerY + 25; y++)
            for (var x = centerX - 21; x <= centerX + 21; x++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var gx = (int)Math.Floor((dx * cos + dy * sin) / scale + 2.5);
                var gy = (int)Math.Floor((-dx * sin + dy * cos) / scale + 3.5);
                if (gx is >= 0 and < 5 && gy is >= 0 and < 7 && (glyph[gy] & (1 << (4 - gx))) != 0)
                    SetPixel(pixels, x, y, 20, 63, 112);
            }
        }

        using var png = new MemoryStream();
        png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, Width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), Height);
        header[8] = 8; // RGB, eight bits per channel.
        header[9] = 2;
        WriteChunk(png, "IHDR", header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(pixels);
        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", []);
        return "data:image/png;base64," + Convert.ToBase64String(png.ToArray());
    }

    private static void SetPixel(byte[] pixels, int x, int y, byte red, byte green, byte blue)
    {
        if ((uint)x >= Width || (uint)y >= Height) return;
        var offset = y * Stride + 1 + x * 3;
        pixels[offset] = red;
        pixels[offset + 1] = green;
        pixels[offset + 2] = blue;
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> number = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, data.Length);
        stream.Write(number);
        var name = Encoding.ASCII.GetBytes(type);
        stream.Write(name);
        stream.Write(data);

        var crc = uint.MaxValue;
        foreach (var value in name) crc = UpdateCrc(crc, value);
        foreach (var value in data) crc = UpdateCrc(crc, value);
        BinaryPrimitives.WriteUInt32BigEndian(number, ~crc);
        stream.Write(number);
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (var i = 0; i < 8; i++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        return crc;
    }
}
