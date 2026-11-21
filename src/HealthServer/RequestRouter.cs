using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthServer
{
    public class RequestRouter
    {
        private readonly HealthRepository _repository;

        public RequestRouter(HealthRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> ProcessRequestAsync(string jsonRequest)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(jsonRequest);
                    JsonElement root = doc.RootElement;

                    string command = root.GetProperty("Command").GetString() ?? "";
                    JsonElement payload = root.GetProperty("Payload");

                    switch (command)
                    {
                        case "SEARCH":
                            string keyword = payload.GetProperty("Keyword").GetString() ?? "";
                            var searchResults = _repository.Search(keyword);
                            return JsonSerializer.Serialize(new
                            {
                                Status = "SUCCESS",
                                Command = "SEARCH_RESPONSE",
                                Data = searchResults
                            });

                        case "GET_DETAIL":
                            int diseaseId = payload.GetProperty("DiseaseId").GetInt32();
                            var diseaseDetail = _repository.GetById(diseaseId);
                            if (diseaseDetail != null)
                            {
                                return JsonSerializer.Serialize(new
                                {
                                    Status = "SUCCESS",
                                    Command = "GET_DETAIL_RESPONSE",
                                    Data = diseaseDetail
                                });
                            }
                            return JsonSerializer.Serialize(new
                            {
                                Status = "ERROR",
                                Message = "Không tìm thấy thông tin bệnh"
                            });

                        default:
                            return JsonSerializer.Serialize(new
                            {
                                Status = "ERROR",
                                Message = "Lệnh không hợp lệ"
                            });
                    }
                }
                catch (Exception ex)
                {
                    return JsonSerializer.Serialize(new
                    {
                        Status = "ERROR",
                        Message = "Lỗi xử lý JSON: " + ex.Message
                    });
                }
            });
        }
    }
}