using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthServer
{
    public class Disease
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Symptoms { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
    }

    public class HealthRepository
    {
        private readonly List<Disease> _diseases = new()
        {
            new Disease
            {
                Id = 1,
                Name = "Cảm cúm (Influenza)",
                Symptoms = new List<string> { "ho", "sốt", "đau đầu", "mệt mỏi", "chảy nước mũi" },
                Description = "Bệnh nhiễm trùng đường hô hấp cấp tính do virus cúm gây ra.",
                Treatment = "Nghỉ ngơi, uống nhiều nước, sử dụng thuốc hạ sốt paracetamol khi cần."
            },
            new Disease
            {
                Id = 2,
                Name = "Viêm họng cấp",
                Symptoms = new List<string> { "ho", "đau họng", "sốt nhẹ", "rát cổ" },
                Description = "Tình trạng viêm nhiễm ở niêm mạc họng.",
                Treatment = "Súc miệng bằng nước muối ấm, uống nước ấm, dùng thuốc giảm đau rát họng."
            },
            new Disease
            {
                Id = 3,
                Name = "Viêm dạ dày cấp",
                Symptoms = new List<string> { "đau bụng", "buồn nôn", "đầy hơi", "ợ chua" },
                Description = "Tình trạng tổn thương sưng viêm ở niêm mạc dạ dày.",
                Treatment = "Ăn thức ăn lỏng dễ tiêu, tránh đồ chua cay, dùng thuốc trung hòa axit theo chỉ định."
            },
            new Disease
            {
                Id = 4,
                Name = "Sốt xuất huyết",
                Symptoms = new List<string> { "sốt", "sốt cao", "đau đầu", "đau hốc mắt", "phát ban" },
                Description = "Bệnh truyền nhiễm cấp tính do virus Dengue gây ra qua muỗi vằn.",
                Treatment = "Tăng cường bù nước (Oresol), theo dõi dõi sát thân nhiệt, đến cơ sở y tế ngay."
            }
        };

        public List<Disease> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return _diseases;

            string lowerKey = keyword.Trim().ToLower();
            return _diseases.Where(d =>
                d.Name.ToLower().Contains(lowerKey) ||
                d.Symptoms.Any(s => s.ToLower().Contains(lowerKey))
            ).ToList();
        }

        public Disease? GetById(int id)
        {
            return _diseases.FirstOrDefault(d => d.Id == id);
        }
    }
}