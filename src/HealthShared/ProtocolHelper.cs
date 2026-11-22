using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace HealthShared
{
    public static class ProtocolHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        };

        private const int MAX_PACKET_SIZE = 65536;

        public static string SerializeData<T>(T data)
        {
            string jsonString = JsonSerializer.Serialize(data, _jsonOptions);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

            if (bytes.Length > MAX_PACKET_SIZE)
            {
                throw new InvalidOperationException($"Payload Too Large: {bytes.Length} bytes exceed 64KiB limit.");
            }

            return jsonString;
        }

        public static T DeserializeData<T>(string jsonString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

            if (bytes.Length > MAX_PACKET_SIZE)
            {
                throw new InvalidOperationException("Payload Too Large: Exceeds 64KiB limit.");
            }

            return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
        }
    }
}