using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthShared;

public static class AuthTcpFraming
{
    private const int MaxPayloadBytes = 64 * 1024;

    public static async Task WriteAsync(
        Stream stream, string json, CancellationToken token = default)
    {
        byte[] payload = Encoding.UTF8.GetBytes(json);
        ValidateLength(payload.Length);

        byte[] header = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);

        await stream.WriteAsync(header.AsMemory(), token);
        await stream.WriteAsync(payload.AsMemory(), token);
    }

    public static async Task<string> ReadAsync(
        Stream stream, CancellationToken token = default)
    {
        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header.AsMemory(), token);

        int length = BinaryPrimitives.ReadInt32BigEndian(header);
        ValidateLength(length);

        byte[] payload = new byte[length];
        await stream.ReadExactlyAsync(payload.AsMemory(), token);
        return Encoding.UTF8.GetString(payload);
    }

    private static void ValidateLength(int length)
    {
        if (length <= 0 || length > MaxPayloadBytes)
        {
            throw new InvalidDataException("Gói JSON phải có từ 1 đến 65536 byte.");
        }
    }
}