using System;
using HealthShared;

namespace HealthServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                BaseResponse response = new BaseResponse
                {
                    Command = "SEARCH_RESPONSE",
                    Status = ErrorCode.Success,
                    Message = "Tìm thấy thông tin bệnh: Triệu chứng ho, sốt, đau họng",
                    Payload = "{\"Disease\":\"Cảm cúm\"}"
                };

                string json = ProtocolHelper.SerializeData(response);
                Console.WriteLine("JSON Output:");
                Console.WriteLine(json);

                BaseResponse decoded = ProtocolHelper.DeserializeData<BaseResponse>(json);
                Console.WriteLine("Decoded Message: " + decoded.Message);

                Console.WriteLine("\nTesting 64KiB limit...");
                BaseRequest largeRequest = new BaseRequest
                {
                    Command = "LARGE_DATA",
                    Payload = new string('A', 70000)
                };

                ProtocolHelper.SerializeData(largeRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception: " + ex.Message);
            }

            Console.ReadLine();
        }
    }
}