-- Tạo cơ sở dữ liệu
CREATE DATABASE IF NOT EXISTS QuanLyBenhHoHap;
USE QuanLyBenhHoHap;

-- 1. Bảng lưu trữ thông tin bệnh lý hô hấp
CREATE TABLE BenhHoHap (
    MaBenh VARCHAR(20) PRIMARY KEY,
    TenBenh NVARCHAR(150) NOT NULL,
    TrieuChung NTEXT NOT NULL,
    MoTa NTEXT NOT NULL,
    KhiNaoCanDiKham NTEXT NOT NULL,
    LinkNguon VARCHAR(255) NOT NULL
);

-- 2. Bảng lưu trữ thuốc tham khảo từ các nhà thuốc
CREATE TABLE ThuocThamKhao (
    MaThuoc INT IDENTITY(1,1) PRIMARY KEY,
    MaBenh VARCHAR(20),
    TenThuoc NVARCHAR(150) NOT NULL,
    NhaThuoc NVARCHAR(100) NOT NULL, -- Ví dụ: FPT Long Châu, Pharmacity
    CachDung NVARCHAR(255),
    FOREIGN KEY (MaBenh) REFERENCES BenhHoHap(MaBenh)
);

-- 3. INSERT dữ liệu danh sách các bệnh hô hấp phổ biến (10 bệnh)
INSERT INTO BenhHoHap (MaBenh, TenBenh, TrieuChung, MoTa, KhiNaoCanDiKham, LinkNguon) VALUES
('BH01', N'Cảm cúm (Influenza)', 
 N'Sốt, ớn lạnh, đau đầu, đau mỏi cơ bắp, mệt mỏi, ho khan, nghẹt mũi hoặc sổ mũi, đau họng.', 
 N'Cảm cúm là một bệnh nhiễm trùng đường hô hấp cấp tính do virus cúm gây ra, lây lan dễ dàng qua đường không khí khi người bệnh ho hoặc hắt hơi.', 
 N'Khi sốt cao kéo dài trên 38.5°C quá 3 ngày, khó thở, tức ngực, hoặc các triệu chứng trở nặng không thuyên giảm sau một tuần.', 
 N'https://www.who.int/news-room/fact-sheets/detail/influenza-(seasonal)'),

('BH02', N'Viêm phổi (Pneumonia)', 
 N'Ho có đờm đặc hoặc có máu, đau ngực khi thở hoặc ho, khó thở, sốt cao kèm vã mồ hôi và ớn lạnh, mệt mỏi.', 
 N'Viêm phổi là tình trạng nhiễm trùng làm tổn thương túi khí ở một hoặc cả hai bên phổi, túi khí có thể đầy mủ hoặc dịch nhầy.', 
 N'Xuất hiện tình trạng khó thở nghiêm trọng, đau ngực dữ dội, môi hoặc móng tay chuyển sang màu tím xanh, sốt cao liên tục không hạ.', 
 N'https://www.vinmec.com/vie/bai-viet/cac-loai-viem-phoi-thuong-gap-vi'),

('BH03', N'Hen suyễn (Asthma)', 
 N'Thở khò khè, khó thở, tức ngực, ho khan đặc biệt là về đêm hoặc sáng sớm khi trời lạnh.', 
 N'Hen suyễn là bệnh mãn tính làm viêm và thu hẹp đường thở, khiến không khí khó lưu thông, dễ bùng phát khi tiếp xúc với dị nguyên.', 
 N'Khi cơn ho khò khè không đáp ứng với thuốc cắt cơn thông thường, môi tím tái, nhịp thở quá nhanh, không thể nói hết câu.', 
 N'https://www.vinmec.com/vie/bai-viet/benh-hen-suyen-nhung-dieu-can-biet-vi'),

('BH04', N'Viêm phế quản cấp (Acute Bronchitis)', 
 N'Ho kéo dài (có đờm trong hoặc màu vàng xanh), sốt nhẹ, mệt mỏi, thở khò khè, rát ngực khi ho.', 
 N'Viêm phế quản cấp là tình trạng viêm niêm mạc của ống phế quản mang không khí đến và đi từ phổi, thường do nhiễm virus sau cảm cúm.', 
 N'Ho kéo dài hơn 3 tuần, sốt cao liên tục trên 38°C, thở khò khè nặng hoặc ho ra máu.', 
 N'https://www.vinmec.com/vie/bai-viet/chan-doan-va-dieu-tri-viem-phe-quan-cap-vi'),

('BH05', N'Bệnh phổi tắc nghẽn mãn tính (COPD)', 
 N'Khó thở mãn tính dai dẳng (tăng dần theo thời gian), ho mãn tính có đờm, thở khò khè, tức ngực.', 
 N'COPD là bệnh lý viêm phổi mãn tính gây cản trở luồng khí từ phổi, thường do hút thuốc lá lâu năm hoặc tiếp xúc với khói bụi độc hại.', 
 N'Khi tình trạng khó thở đột ngột nghiêm trọng hơn, đờm đổi màu hoặc tăng lượng đờm đáng kể, môi tím tái.', 
 N'https://www.vinmec.com/vie/bai-viet/benh-phoi-tac-nghen-man-tinh-copd-vi'),

('BH06', N'Viêm xoang (Sinusitis)', 
 N'Đau tức vùng mặt (trán, má, giữa hai mắt), nghẹt mũi, chảy mũi đờm đặc màu xanh/vàng, giảm khứu giác, ho về đêm.', 
 N'Viêm xoang là tình trạng viêm hoặc sưng đau ở niêm mạc lót trong các xoang cạnh mũi, làm tắc nghẽn dịch nhầy.', 
 N'Đau đầu dữ dội không giảm, sưng đỏ quanh mắt, nhìn đôi (nhìn 1 thành 2), cổ cứng hoặc sốt cao.', 
 N'https://www.vinmec.com/vie/benh/viem-xoang-2953'),

('BH07', N'Viêm họng cấp (Acute Pharyngitis)', 
 N'Đau họng, nuốt đau, họng đỏ sưng, sốt, đau đầu, nổi hạch ở cổ, ho khan.', 
 N'Viêm họng cấp là tình trạng viêm nhiễm ở niêm mạc họng, phổ biến do nhiễm virus (cúm, Adenovirus) hoặc vi khuẩn (liên cầu khuẩn nhóm A).', 
 N'Đau họng dữ dội không thể nuốt nước bọt, khó thở, sốt cao trên 39°C, hoặc đau họng kéo dài trên 1 tuần.', 
 N'https://www.vinmec.com/vie/benh/viem-hong-cap-2952'),

('BH08', N'Viêm thanh quản (Laryngitis)', 
 N'Khàn tiếng, mất tiếng, cảm giác vướng họng, ho khan, sốt nhẹ.', 
 N'Viêm thanh quản xảy ra khi dây thanh âm bị viêm, sưng nề do nói nhiều, la hét, nhiễm trùng đường hô呼吸 hoặc hít phải chất kích thích.', 
 N'Mất tiếng hoặc khàn tiếng kéo dài trên 2 tuần, khó thở, ho ra máu, khó nuốt nặng.', 
 N'https://www.vinmec.com/vie/benh/viem-thanh-quan-3165'),

('BH09', N'Lao phổi (Pulmonary Tuberculosis)', 
 N'Ho kéo dài trên 3 tuần (có thể ho ra máu), sốt nhẹ về chiều, đổ mồ hôi đêm, sút cân không rõ nguyên nhân, mệt mỏi.', 
 N'Lao phổi là bệnh truyền nhiễm do vi khuẩn Mycobacterium tuberculosis gây ra, lây qua đường hô hấp khi người bệnh ho hoặc hắt hơi.', 
 N'Cần đi khám ngay khi ho kéo dài trên 2-3 tuần, ho ra máu, sốt về chiều và sụt cân nhanh chóng.', 
 N'https://www.vinmec.com/vie/benh/benh-lao-phoi-3183'),

('BH10', N'Viêm mũi dị ứng (Allergic Rhinitis)', 
 N'Hắt hơi liên tục thành tràng, ngứa mũi, ngứa mắt, chảy nước mũi trong, nghẹt mũi.', 
 N'Viêm mũi dị ứng là phản ứng miễn dịch của cơ thể khi hít phải các chất gây dị ứng như phấn hoa, bụi nhà, lông thú nuôi, nấm mốc.', 
 N'Triệu chứng kéo dài ảnh hưởng nghiêm trọng đến giấc ngủ, sinh hoạt, hoặc có dấu hiệu biến chứng sang viêm xoang, viêm tai giữa.', 
 N'https://www.vinmec.com/vie/benh/viem-mui-di-ung-4751');

-- 4. INSERT dữ liệu thuốc tham khảo từ Nhà thuốc FPT Long Châu và Pharmacity
INSERT INTO ThuocThamKhao (MaBenh, TenThuoc, NhaThuoc, CachDung) VALUES
-- Cảm cúm (BH01)
('BH01', N'Paracetamol 500mg', N'FPT Long Châu', N'Uống 1 viên/lần khi sốt, cách nhau từ 4-6 tiếng'),
('BH01', N'Decolgen Forte', N'Pharmacity', N'Uống 1 viên x 3 lần/ngày sau khi ăn'),
('BH01', N'Vitamin C 1000mg', N'FPT Long Châu', N'Uống 1 viên pha loãng với nước vào ban ngày'),

-- Viêm phổi (BH02)
('BH02', N'Augmentin 1g', N'FPT Long Châu', N'Uống theo chỉ định nghiêm ngặt của bác sĩ (thường 2 lần/ngày)'),
('BH02', N'Alpha Choay', N'Pharmacity', N'Ngậm dưới lưỡi hoặc uống theo hướng dẫn bác sĩ'),

-- Hen suyễn (BH03)
('BH03', N'Ventolin Inhaler 100mcg', N'FPT Long Châu', N'Xịt cắt cơn theo hướng dẫn khi có triệu chứng khó thở khò khè'),
('BH03', N'Seretide Diskus', N'Pharmacity', N'Hít dự phòng hàng ngày theo đơn của bác sĩ chuyên khoa'),

-- Viêm phế quản cấp (BH04)
('BH04', N'Bisolvon 8mg', N'FPT Long Châu', N'Uống 1 viên x 3 lần/ngày để long đờm'),
('BH04', N'Eugica Fort', N'Pharmacity', N'Uống 1-2 viên x 3 lần/ngày giúp giảm ho, dịu họng'),

-- COPD (BH05)
('BH05', N'Spiriva Respimat 2.5mcg', N'FPT Long Châu', N'Hít 2 nhát x 1 lần/ngày theo chỉ định bác sĩ'),
('BH05', N'Symbicort Turbuhaler 160/4.5mcg', N'Pharmacity', N'Hít duy trì hàng ngày theo chỉ định của bác sĩ chuyên khoa'),

-- Viêm xoang (BH06)
('BH06', N'Xịt mũi Xysal / Otrivin 0.1%', N'FPT Long Châu', N'Xịt mỗi bên mũi 1 lần, không dùng quá 7 ngày liên tục'),
('BH06', N'Thông Xoang Nam Dược', N'Pharmacity', N'Uống 4 viên/ngày chia 2 lần sau khi ăn'),

-- Viêm họng cấp (BH07)
('BH07', N'Viên ngậm Strepsils', N'Pharmacity', N'Ngậm 1 viên cách nhau 2-3 giờ khi đau họng'),
('BH07', N'Siro Prospan', N'FPT Long Châu', N'Uống 5ml x 3 lần/ngày giúp giảm ho, dịu niêm mạc họng'),

-- Viêm thanh quản (BH08)
('BH08', N'Bảo Thanh viên ngậm', N'FPT Long Châu', N'Ngậm 1 viên khi bị khàn tiếng, rát họng'),
('BH08', N'Xịt họng Nhất Nhất', N'Pharmacity', N'Xịt vào họng 2-3 lần/ngày'),

-- Lao phổi (BH09)
('BH09', N'Turmocox 4 (Thuốc kháng lao)', N'FPT Long Châu', N'Dùng nghiêm ngặt theo phác đồ điều trị của Chương trình Chống lao Quốc gia'),

-- Viêm mũi dị ứng (BH10)
('BH10', N'Telfast HD 180mg / Loratadin 10mg', N'Pharmacity', N'Uống 1 viên/ngày vào buổi sáng'),
('BH10', N'Xịt mũi Flixonase / Avamys', N'FPT Long Châu', N'Xịt mỗi bên mũi 1-2 nhát/ngày');
