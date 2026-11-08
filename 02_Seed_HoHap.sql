-- =========================================================================
-- SCRIPT GỘP TOÀN BỘ SEED DATA: NHÓM BỆNH HÔ HẤP CHO HỆ THỐNG TRA CỨU SỨC KHỎE
-- =========================================================================

-- 1. Thêm danh mục chuyên khoa Hô hấp
INSERT INTO `dbo`.`Categories` (`CategoryID`, `TenDanhMuc`, `MoTa`, `IsActive`) VALUES
(1, N'Bệnh Hô Hấp', N'Các bệnh lý liên quan đến đường hô hấp trên và dưới như mũi, họng, phế quản và phổi.', 1);

-- 2. Thêm tài khoản quản trị mẫu (dùng cho trường NguoiCapNhat)
INSERT INTO `dbo`.`Users` (`UserID`, `TenDangNhap`, `MatKhauBam`, `Salt`, `HoTen`, `VaiTro`) VALUES
(1, 'admin_content', 'dummy_hash_string_64_chars_length_for_testing_purposes_abc123', 'dummy_salt_32_chars', N'Nguyễn Quản Trị Nội Dung', 'ContentAdmin');

-- 3. Thêm toàn bộ các triệu chứng thuộc hệ hô hấp (SymptomID từ 1 đến 15)
INSERT INTO `dbo`.`Symptoms` (`SymptomID`, `TenTrieuChung`, `MoTa`, `Nguon`, `IsActive`) VALUES
(1, N'Sốt cao', N'Nhiệt độ cơ thể tăng cao trên 38.5 độ C, kèm theo cảm giác ớn lạnh.', N'Bộ Y tế', 1),
(2, N'Ho khan, ho có đờm', N'Tình trạng ho kéo dài, có thể kèm theo đờm trắng hoặc vàng xanh.', N'WHO', 1),
(3, N'Khó thở, thở khò khè', N'Cảm giác hụt hơi, thở ngắn, phát ra tiếng rít khi thở ra hoặc hít vào.', N'Bệnh viện Chợ Rẫy', 1),
(4, N'Đau rát họng', N'Cảm giác đau, ngứa rát vùng cổ họng khi nuốt nước bọt hoặc ăn uống.', N'MSD Manual', 1),
(5, N'Đau tức ngực', N'Cảm giác nặng ngực, đau tức âm ỉ hoặc nhói ở vùng lồng ngực khi hít thở sâu.', N'Bệnh viện Tâm Anh', 1),
(6, N'Sổ mũi, nghẹt mũi', N'Chảy nước mũi trong hoặc đặc, nghẹt một hoặc cả hai bên lỗ mũi.', N'Bộ Y tế', 1),
(7, N'Đau họng, nuốt đau', N'Cảm giác vướng víu, đau rát vùng họng khi ăn uống hoặc nuốt nước bọt.', N'WHO', 1),
(8, N'Hắt hơi liên tục', N'Các cơn hắt hơi đột ngột do kích ứng niêm mạc mũi.', N'Bệnh viện Chợ Rẫy', 1),
(9, N'Mệt mỏi, suy nhược', N'Cảm giác kiệt sức, thiếu năng lượng toàn thân khi nhiễm trùng.', N'MSD Manual', 1),
(10, N'Đau đầu, nhức mỏi cơ khớp', N'Đau ê ẩm vùng đầu, cơ bắp và các khớp khi bị sốt virus.', N'Bộ Y tế', 1),
(11, N'Khạc đờm xanh hoặc vàng', N'Ho tống ra đờm có màu sắc bất thường do nhiễm khuẩn đường thở.', N'Bệnh viện Tâm Anh', 1),
(12, N'Thở rít, thở khò khè', N'Âm thanh phát ra khi không khí đi qua đường thở bị hẹp.', N'Bệnh viện Chợ Rẫy', 1),
(13, N'Khàn tiếng, mất tiếng', N'Tổn thương dây thanh quản gây thay đổi âm sắc giọng nói.', N'Bộ Y tế', 1),
(14, N'Tức ngực, khó thở khi gắng sức', N'Khó thở tăng lên rõ rệt khi vận động mạnh hoặc leo cầu thang.', N'Bệnh viện Tâm Anh', 1),
(15, N'Sụt cân không rõ nguyên nhân', N'Giảm cân nhanh trong thời gian ngắn do bệnh lý mạn tính tiêu hao.', N'WHO', 1);

-- 4. Thêm toàn bộ 13 Bệnh lý Hô hấp (DiseaseID từ 1 đến 13)
INSERT INTO `dbo`.`Diseases` (`DiseaseID`, `CategoryID`, `TenBenh`, `MaICD`, `TongQuan`, `NguyenNhan`, `PhongNgua`, `CanhBao`, `MucDoPhoBien`, `Nguon`, `NgayTruyCap`, `IsPublished`, `NguoiCapNhat`) VALUES
(1, 1, N'Cảm cúm', 'J11', 
 N'Cảm cúm là bệnh nhiễm trùng đường hô hấp cấp tính do virus cúm gây ra, lây lan dễ dàng qua đường không khí khi người bệnh ho hoặc hắt hơi.', 
 N'Do các chủng virus cúm A, cúm B hoặc cúm C xâm nhập qua đường hô hấp.', 
 N'Tiêm vaccine cúm định kỳ hàng năm, đeo khẩu trang nơi công cộng, rửa tay thường xuyên bằng xà phòng.', 
 N'Cẩn trọng với biến chứng viêm phổi ở trẻ em, người cao tuổi và người có bệnh nền.', 
 N'Rất thường gặp', N'Bộ Y tế / WHO', '2026-03-01', 1, 1),

(2, 1, N'Viêm phổi', 'J18', 
 N'Viêm phổi là tình trạng nhiễm trùng làm tổn thương các túi khí ở một hoặc cả hai bên phổi, khiến túi khí chứa đầy dịch hoặc mủ gây ho, sốt và khó thở.', 
 N'Do vi khuẩn (phế cầu khuẩn), virus hoặc nấm tấn công hệ hô hấp dưới.', 
 N'Giữ ấm cơ thể khi thời tiết chuyển mùa, bỏ hút thuốc lá, tiêm vaccine phế cầu và cúm.', 
 N'Viêm phổi nặng có thể gây suy hô hấp nguy hiểm đến tính mạng nếu không được điều trị kịp thời.', 
 N'Thường gặp', N'Bệnh viện Chợ Rẫy', '2026-03-02', 1, 1),

(3, 1, N'Hen suyễn', 'J45', 
 N'Hen suyễn là bệnh lý viêm mạn tính đường mạn phế quản, làm tăng độ nhạy cảm và gây co thắt đường thở dẫn đến các cơn khó thở đột ngột.', 
 N'Do dị ứng với phấn hoa, khói bụi, lông thú cưng, thời tiết lạnh hoặc di truyền.', 
 N'Tránh tiếp xúc với các dị nguyên, giữ môi trường sống sạch sẽ, luôn mang theo thuốc cắt cơn theo chỉ định bác sĩ.', 
 N'Cơn hen cấp tính nặng có thể dẫn đến ngạt thở, tím tái và tử vong nhanh chóng.', 
 N'Thường gặp', N'WHO / Bệnh viện Tâm Anh', '2026-03-03', 1, 1),

(4, 1, N'Viêm họng cấp', 'J02', 
 N'Viêm họng cấp là tình trạng viêm niêm mạc và các tổ chức bạch huyết của họng, thường xuất hiện đột ngột khi thời tiết thay đổi.', 
 N'Do virus (Adenovirus, Rhinovirus) hoặc vi khuẩn (liên cầu khuẩn nhóm A) gây ra.', 
 N'Giữ ấm cổ họng, súc miệng nước muối sinh hoạt hàng ngày, tránh uống nước đá lạnh.', 
 N'Biến chứng sốt thấp khớp hoặc viêm cầu thận nếu do liên cầu khuẩn nhóm A mà không điều trị.', 
 N'Rất thường gặp', N'Bộ Y tế', '2026-03-04', 1, 1),

(5, 1, N'Viêm phế quản cấp', 'J20', 
 N'Viêm phế quản cấp là tình trạng viêm niêm mạc ống phế quản dẫn khí vào phổi, gây ho nhiều kèm đờm.', 
 N'Phần lớn do virus cảm lạnh hoặc cúm tấn công sau khi nhiễm trùng đường hô hấp trên.', 
 N'Bỏ hút thuốc lá, tránh khói bụi ô nhiễm, tiêm vaccine phòng cúm và phế cầu.', 
 N'Có thể tiến triển thành viêm phổi nặng ở người có hệ miễn dịch suy yếu.', 
 N'Rất thường gặp', N'Bệnh viện Chợ Rẫy', '2026-03-04', 1, 1),

(6, 1, N'Viêm mũi dị ứng', 'J30', 
 N'Viêm mũi dị ứng là phản ứng của hệ miễn dịch khi hít phải các dị nguyên trong không khí như phấn hoa, bụi nhà.', 
 N'Do cơ địa dị ứng tiếp xúc với khói bụi, lông động vật, thời tiết lạnh.', 
 N'Vệ sinh nhà cửa sạch sẽ, đeo khẩu trang khi ra đường, tránh xa các tác nhân gây dị ứng.', 
 N'Làm giảm chất lượng cuộc sống, dễ dẫn đến biến chứng viêm xoang mạn tính.', 
 N'Rất thường gặp', N'WHO / Bệnh viện Tâm Anh', '2026-03-04', 1, 1),

(7, 1, N'Viêm xoang mạn tính', 'J32', 
 N'Viêm xoang mạn tính là tình trạng sưng viêm các hốc xoang xung quanh mũi kéo dài trên 12 tuần không khỏi.', 
 N'Do viêm mũi dị ứng kéo dài, polyp mũi, vách ngăn mũi lệch hoặc nhiễm trùng vi khuẩn.', 
 N'Điều trị dứt điểm viêm mũi, giữ độ ẩm không khí trong phòng, tránh khói thuốc.', 
 N'Nhiễm trùng có thể lan sang ổ mắt hoặc màng não nếu không điều trị đúng cách.', 
 N'Thường gặp', N'Bệnh viện Tâm Anh', '2026-03-04', 1, 1),

(8, 1, N'Bệnh phổi tắc nghẽn mạn tính (COPD)', 'J44', 
 N'COPD là bệnh lý mạn tính gây cản trở lưu thông không khí từ phổi, làm người bệnh khó thở tăng dần theo thời gian.', 
 N'Hút thuốc lá lâu năm là nguyên nhân hàng đầu, ngoài ra do khói bụi nghề nghiệp, ô nhiễm.', 
 N'Tuyệt đối không hút thuốc lá, tránh tiếp xúc khói bếp than, tập thể dục hô hấp đều đặn.', 
 N'Gây suy hô hấp mạn tính, ảnh hưởng nghiêm trọng đến tim mạch và tuổi thọ.', 
 N'Thường gặp', N'Bộ Y tế / WHO', '2026-03-04', 1, 1),

(9, 1, N'Hen phế quản bội nhiễm', 'J45.9', 
 N'Dạng hen phế quản có kèm theo tình trạng nhiễm khuẩn đường thở, làm tăng tiết đờm nhầy và khó thở nặng.', 
 N'Do thời tiết thay đổi lạnh ẩm kết hợp vi khuẩn xâm nhập vào đường thở bị viêm sẵn.', 
 N'Tuân thủ phác đồ điều trị dự phòng hen, tránh xa khói thuốc lá và dị nguyên.', 
 N'Có nguy cơ cao dẫn đến cơn hen ác tính gây tử vong nếu chậm trễ cấp cứu.', 
 N'Thường gặp', N'Bệnh viện Chợ Rẫy', '2026-03-04', 1, 1),

(10, 1, N'Lao phổi', 'A15', 
 N'Lao phổi là bệnh truyền nhiễm nguy hiểm do trực khuẩn lao tấn công và phá hủy các mô phổi.', 
 N'Do vi khuẩn Mycobacterium tuberculosis lây qua đường không khí khi người bệnh ho, khạc.', 
 N'Tiêm vaccine BCG cho trẻ sơ sinh, cách ly nguồn lây, giữ nhà cửa thông thoáng nhiều ánh nắng.', 
 N'Lây lan mạnh trong cộng đồng, gây ho ra máu và hoại tử mô phổi nếu không tuân thủ phác đồ dài hạn.', 
 N'Ít gặp', N'Chương trình Chống lao Quốc gia / Bộ Y tế', '2026-03-04', 1, 1),

(11, 1, N'Áp xe phổi', 'J85', 
 N'Áp xe phổi là ổ hoại tử mủ khu trú trong nhu mô phổi hình thành do ổ nhiễm trùng nặng.', 
 N'Biến chứng của viêm phổi do vi khuẩn kỵ khí hoặc do hít phải dị vật, dịch dạ dày.', 
 N'Điều trị sớm và dứt điểm các ổ nhiễm trùng đường hô hấp trên và dưới.', 
 N'Ổ áp xe có thể vỡ vào màng phổi gây tràn mủ màng phổi rất nguy hiểm.', 
 N'Hiếm gặp', N'Bệnh viện Chợ Rẫy', '2026-03-04', 1, 1),

(12, 1, N'Tràn dịch màng phổi', 'J90', 
 N'Tình trạng tích tụ dịch bất thường khoang màng phổi làm ép xẹp phổi, gây khó thở dữ dội.', 
 N'Do biến chứng viêm phổi, lao màng phổi, suy tim hoặc ung thư phổi.', 
 N'Khám sức khỏe định kỳ, điều trị triệt để các bệnh lý nhiễm trùng nguyên phát.', 
 N'Gây chèn ép tim phổi cấp tính, suy hô hấp đe dọa tính mạng.', 
 N'Ít gặp', N'Bệnh viện Tâm Anh', '2026-03-04', 1, 1),

(13, 1, N'Viêm thanh quản cấp', 'J04', 
 N'Viêm niêm mạc thanh quản cấp tính dẫn đến hiện tượng khàn tiếng hoặc mất giọng hoàn toàn.', 
 N'Do sử dụng giọng quá mức (la hét, nói nhiều) hoặc nhiễm virus đường hô hấp trên.', 
 N'Hạn chế nói to, uống nhiều nước ấm, tránh hút thuốc lá và uống rượu bia.', 
 N'Có thể gây khó thở thanh quản cấp ở trẻ nhỏ (cần cấp cứu kịp thời).', 
 N'Thường gặp', N'Bộ Y tế', '2026-03-04', 1, 1);

-- 5. Thêm toàn bộ liên kết Bệnh lý với Triệu chứng (DiseaseSymptoms)
INSERT INTO `dbo`.`DiseaseSymptoms` (`DiseaseID`, `SymptomID`, `MoTa`, `MucDoLienQuan`, `Nguon`, `NgayTruyCap`) VALUES
-- Cảm cúm (DiseaseID = 1)
(1, 1, N'Sốt cao đột ngột từ 38-39 độ C', 4, N'Bộ Y tế', '2026-03-01'),
(1, 2, N'Ho khan hoặc ho ít đờm', 3, N'Bộ Y tế', '2026-03-01'),
(1, 4, N'Đau rát họng khi nuốt', 5, N'Bộ Y tế', '2026-03-01'),

-- Viêm phổi (DiseaseID = 2)
(2, 1, N'Sốt cao kéo dài, rét run', 5, N'Bệnh viện Chợ Rẫy', '2026-03-02'),
(2, 2, N'Ho có đờm đặc, màu vàng hoặc xanh', 5, N'Bệnh viện Chợ Rẫy', '2026-03-02'),
(2, 3, N'Khó thở, thở gấp, hụt hơi', 5, N'Bệnh viện Chợ Rẫy', '2026-03-02'),
(2, 5, N'Đau tức ngực tăng lên khi ho hoặc hít sâu', 4, N'Bệnh viện Chợ Rẫy', '2026-03-02'),

-- Hen suyễn (DiseaseID = 3)
(3, 2, N'Ho khan dai dẳng, nhất là về đêm hoặc sáng sớm', 3, N'Bệnh viện Tâm Anh', '2026-03-03'),
(3, 3, N'Khó thở khò khè, nghe tiếng rít rõ rệt', 5, N'Bệnh viện Tâm Anh', '2026-03-03'),
(3, 5, N'Cảm giác thắt chặt, tức nặng lồng ngực', 4, N'Bệnh viện Tâm Anh', '2026-03-03'),

-- Viêm họng cấp (DiseaseID = 4)
(4, 4, N'Đau rát họng tăng mạnh khi nuốt', 5, N'Bộ Y tế', '2026-03-04'),
(4, 7, N'Cảm giác vướng và đau họng rõ rệt', 5, N'Bộ Y tế', '2026-03-04'),
(4, 9, N'Mệt mỏi, uể oải', 3, N'Bộ Y tế', '2026-03-04'),

-- Viêm phế quản cấp (DiseaseID = 5)
(5, 2, N'Ho khan chuyển sang ho có đờm đục', 5, N'Bệnh viện Chợ Rẫy', '2026-03-04'),
(5, 9, N'Mệt mỏi toàn thân do ho nhiều', 3, N'Bệnh viện Chợ Rẫy', '2026-03-04'),
(5, 11, N'Khạc đờm vàng xanh', 4, N'Bệnh viện Chợ Rẫy', '2026-03-04'),

-- Viêm mũi dị ứng (DiseaseID = 6)
(6, 6, N'Sổ mũi nước trong liên tục', 5, N'Bệnh viện Tâm Anh', '2026-03-04'),
(6, 8, N'Hắt hơi từng tràng vào buổi sáng sớm', 5, N'Bệnh viện Tâm Anh', '2026-03-04'),

-- Viêm xoang mạn tính (DiseaseID = 7)
(7, 6, N'Nghẹt mũi, chảy dịch mũi mủ xanh', 5, N'Bệnh viện Tâm Anh', '2026-03-04'),
(7, 10, N'Đau nhức vùng trán và hốc mắt', 4, N'Bệnh viện Tâm Anh', '2026-03-04'),

-- COPD (DiseaseID = 8)
(8, 3, N'Khó thở mạn tính, tăng dần theo thời gian', 5, N'Bộ Y tế', '2026-03-04'),
(8, 14, N'Tức ngực, khó thở khi làm việc nặng', 5, N'Bộ Y tế', '2026-03-04'),
(8, 15, N'Sụt cân ở giai đoạn nặng', 3, N'Bộ Y tế', '2026-03-04'),

-- Hen phế quản bội nhiễm (DiseaseID = 9)
(9, 3, N'Cơn khó thở khò khè đột ngột', 5, N'Bệnh viện Chợ Rẫy', '2026-03-04'),
(9, 11, N'Ho khạc đờm đặc do bội nhiễm vi khuẩn', 4, N'Bệnh viện Chợ Rẫy', '2026-03-04'),

-- Lao phổi (DiseaseID = 10)
(10, 2, N'Ho kéo dài trên 3 tuần không dứt', 5, N'Chương trình Chống lao', '2026-03-04'),
(10, 9, N'Sốt nhẹ về chiều, đổ mồ hôi trộm', 4, N'Chương trình Chống lao', '2026-03-04'),
(10, 15, N'Sụt cân nhanh, gầy sút', 5, N'Chương trình Chống lao', '2026-03-04'),

-- Áp xe phổi (DiseaseID = 11)
(11, 1, N'Sốt cao rét run liên tục', 5, N'Bệnh viện Chợ Rẫy', '2026-03-04'),
(11, 11, N'Ho khạc lượng lớn đờm mủ hôi thối', 5, N'Bệnh viện Chợ Rẫy', '2026-03-04'),

-- Tràn dịch màng phổi (DiseaseID = 12)
(12, 3, N'Khó thở tăng dần, tức nặng một bên ngực', 5, N'Bệnh viện Tâm Anh', '2026-03-04'),
(12, 5, N'Đau tức ngực rõ rệt khi nằm nghiêng', 4, N'Bệnh viện Tâm Anh', '2026-03-04'),

-- Viêm thanh quản cấp (DiseaseID = 13)
(13, 13, N'Khàn tiếng nặng, có khi mất giọng hoàn toàn', 5, N'Bộ Y tế', '2026-03-04'),
(13, 7, N'Cảm giác khô rát, vướng ở cổ họng', 3, N'Bộ Y tế', '2026-03-04');

-- 6. Thêm toàn bộ các loại thuốc tham khảo từ FPT Long Châu và Pharmacity (MedicineID từ 1 đến 12)
INSERT INTO `dbo`.`Medicines` (`MedicineID`, `TenThuoc`, `HoatChat`, `DangBaoChe`, `ChiDinh`, `ChongChiDinh`, `TacDungPhu`, `CanHao`, `CanKeDon`, `Nguon`, `NgayTruyCap`, `IsActive`) VALUES
(1, N'Paracetamol 500mg', N'Paracetamol (Acetaminophen)', N'Viên nén', N'Giảm đau, hạ sốt từ nhẹ đến trung bình.', N'Mẫn cảm với Paracetamol, suy gan nặng.', NULL, NULL, 0, N'Nhà thuốc FPT Long Châu', '2026-03-01', 1),
(2, N'Decolgen Forte', N'Paracetamol, Phenylephrine HCl, Chlorpheniramine maleate', N'Viên nén dài', N'Điều trị các triệu chứng cảm lạnh, nghẹt mũi, sổ mũi, sốt và đau đầu.', N'Quá mẫn với thành phần thuốc, cao huyết áp nặng, bệnh mạch vành.', NULL, NULL, 0, N'Nhà thuốc Pharmacity', '2026-03-01', 1),
(3, N'Acetylcystein 200mg', N'Acetylcystein', N'Thuốc bột sủi bọt', N'Thuốc tiêu chất nhầy trong các bệnh viêm phế quản cấp và mạn tính.', N'Tiền sử loét dạ dày, trẻ em dưới 2 tuổi.', NULL, NULL, 0, N'Nhà thuốc FPT Long Châu', '2026-03-02', 1),
(4, N'Ventolin Inhaler 100mcg', N'Salbutamol sulfat', N'Hít định liều dạng khí dung', N'Cắt và phòng ngừa cơn co thắt phế quản ở người bệnh hen suyễn.', N'Quá mẫn với Salbutamol, rối loạn nhịp tim nhanh.', NULL, NULL, 1, N'Nhà thuốc Pharmacity', '2026-03-03', 1),
(5, N'Tiffy Dey', N'Paracetamol, Chlorpheniramine maleate, Phenylephrine HCl', N'Viên nén', N'Giảm nhanh các triệu chứng cảm cúm, sổ mũi, nghẹt mũi, đau đầu.', N'Quá mẫn với thành phần, suy gan thận nặng, cao huyết áp.', NULL, NULL, 0, N'Nhà thuốc Pharmacity', '2026-03-04', 1),
(6, N'Cloramphenicol / Alpha Choay', N'Chymotrypsin / Trypsin', N'Viên nén ngậm', N'Kháng viêm giảm phù nề dạng men trong các bệnh viêm họng, viêm đường hô hấp.', N'Mẫn cảm với hoạt chất, bệnh nhân giảm huyết áp.', NULL, NULL, 0, N'Nhà thuốc FPT Long Châu', '2026-03-04', 1),
(7, N'Bisolvon 8mg', N'Bromhexin hydroclorid', N'Viên nén', N'Thuốc làm loãng đờm trong các bệnh viêm phế quản cấp và mạn tính.', N'Loét dạ dày tiến triển, phụ nữ có thai 3 tháng đầu.', NULL, NULL, 0, N'Nhà thuốc Pharmacity', '2026-03-04', 1),
(8, N'Clarinase', N'Loratadine, Pseudoephedrine sulfate', N'Viên nén phóng thích chậm', N'Giảm triệu chứng nghẹt mũi, hắt hơi, sổ mũi trong viêm mũi dị ứng.', N'Trẻ em dưới 12 tuổi, cao huyết áp nặng, mạch nhanh.', NULL, NULL, 1, N'Nhà thuốc FPT Long Châu', '2026-03-04', 1),
(9, N'Seretide Diskus 50/250mcg', N'Salmeterol / Fluticasone propionate', N'Bột hít định liều', N'Điều trị thường xuyên bệnh hen phế quản và COPD.', N'Quá mẫn cảm với thành phần thuốc, cơn hen cấp tính nặng.', NULL, NULL, 1, N'Nhà thuốc Pharmacity', '2026-03-04', 1),
(10, N'Augmentin 1g', N'Amoxicillin / Acid Clavulanic', N'Viên nén bao phim', N'Kháng sinh điều trị các nhiễm khuẩn đường hô hấp do vi khuẩn nhạy cảm.', N'Dị ứng với kháng sinh nhóm Penicillin hoặc Cephalosporin.', NULL, NULL, 1, N'Nhà thuốc FPT Long Châu', '2026-03-04', 1),
(11, N'Medrol 16mg', N'Methylprednisolon', N'Viên nén', N'Kháng viêm mạnh, ức chế miễn dịch trong các đợt cấp hen hoặc viêm phế quản nặng.', N'Nhiễm nấm toàn thân, loét dạ dày tá tràng đang tiến triển.', NULL, NULL, 1, N'Nhà thuốc Pharmacity', '2026-03-04', 1),
(12, N'Rhinathiol Promethazine', N'Carbocisteine, Promethazine', N'Siro ho', N'Trị ho khan, ho có đờm, viêm đường hô hấp kèm tiết dịch nhầy.', N'Trẻ em dưới 2 tuổi, suy hô hấp.', NULL, NULL, 0, N'Nhà thuốc FPT Long Châu', '2026-03-04', 1);

-- 7. Thêm toàn bộ liên kết Bệnh lý với Thuốc tham khảo (DiseaseMedicines)
INSERT INTO `dbo`.`DiseaseMedicines` (`DiseaseID`, `MedicineID`, `GhiChu`, `Nguon`, `NgayTruyCap`) VALUES
-- Cảm cúm (DiseaseID = 1)
(1, 1, N'Uống 1 viên/lần khi sốt cao trên 38.5 độ C, cách nhau từ 4-6 tiếng. Mua tại FPT Long Châu.', N'FPT Long Châu', '2026-03-01'),
(1, 2, N'Uống 1 viên x 3 lần/ngày sau khi ăn giúp giảm nghẹt mũi và đau đầu. Mua tại Pharmacity.', N'Pharmacity', '2026-03-01'),

-- Viêm phổi (DiseaseID = 2)
(2, 1, N'Hạ sốt và giảm đau nhức cơ thể theo chỉ định bác sĩ.', N'FPT Long Châu', '2026-03-02'),
(2, 3, N'Pha 1 gói với nước uống sau bữa ăn giúp loãng đờm, dễ khạc đờm.', N'FPT Long Châu', '2026-03-02'),

-- Hen suyễn (DiseaseID = 3)
(4, 4, N'Xịt 1-2 nhát khi có cơn khó thở cấp hoặc dùng theo chỉ định dự phòng của bác sĩ. (Thuốc kê đơn)', N'Pharmacity', '2026-03-03'),

-- Viêm họng cấp (DiseaseID = 4)
(4, 6, N'Ngậm 1 viên x 4 lần/ngày giúp giảm sưng đau họng. Mua tại FPT Long Châu.', N'FPT Long Châu', '2026-03-04'),
(4, 1, N'Uống hạ sốt khi sốt trên 38.5 độ C.', N'FPT Long Châu', '2026-03-04'),

-- Viêm phế quản cấp (DiseaseID = 5)
(5, 7, N'Uống 1 viên x 3 lần/ngày giúp tiêu đờm, dễ khạc nhầy. Mua tại Pharmacity.', N'Pharmacity', '2026-03-04'),
(5, 12, N'Uống 15ml/lần x 3 lần/ngày giảm ho đờm.', N'FPT Long Châu', '2026-03-04'),

-- Viêm mũi dị ứng (DiseaseID = 6)
(6, 5, N'Uống 1 viên/lần giúp giảm hắt hơi và sổ mũi nhanh chóng.', N'Pharmacity', '2026-03-04'),
(6, 8, N'Uống 1 viên/ngày vào buổi sáng giúp kiểm soát dị ứng.', N'FPT Long Châu', '2026-03-04'),

-- Viêm xoang mạn tính (DiseaseID = 7)
(7, 8, N'Uống 1 viên/ngày chống sung huyết mũi xoang.', N'FPT Long Châu', '2026-03-04'),

-- COPD (DiseaseID = 8)
(8, 9, N'Hít định liều theo chỉ định cố định hàng ngày của bác sĩ chuyên khoa.', N'Pharmacity', '2026-03-04'),

-- Hen phế quản bội nhiễm (DiseaseID = 9)
(9, 10, N'Kháng sinh uống theo đơn bác sĩ điều trị bội nhiễm vi khuẩn.', N'FPT Long Châu', '2026-03-04'),
(9, 11, N'Uống theo đợt ngắn ngày theo chỉ định để kháng viêm cấp.', N'Pharmacity', '2026-03-04'),

-- Lao phổi (DiseaseID = 10)
(10, 10, N'Phối hợp kháng sinh đặc trị theo phác đồ trực tiếp nghiêm ngặt.', N'Chương trình Chống lao', '2026-03-04'),

-- Áp xe phổi (DiseaseID = 11)
(11, 10, N'Sử dụng kháng sinh liều cao đường tĩnh mạch/uống theo chỉ định bệnh viện.', N'Bệnh viện Chợ Rẫy', '2026-03-04'),

-- Tràn dịch màng phổi (DiseaseID = 12)
(12, 1, N'Giảm đau, hạ sốt khi có triệu chứng đi kèm.', N'Bệnh viện Tâm Anh', '2026-03-04'),

-- Viêm thanh quản cấp (DiseaseID = 13)
(13, 6, N'Ngậm viên kháng viêm giảm phù nề thanh quản.', N'FPT Long Châu', '2026-03-04');
