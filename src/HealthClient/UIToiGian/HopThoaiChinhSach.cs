using System.Drawing;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Hop thoai "Chinh sach quyen rieng tu" Ban 7 (toi gian, tinh gon):
    //  - 3 y tom tat dau trang chi dung dau cham tron (khong dong khung o vien)
    //  - Khoi tom tat toi gian voi duong ke vien manh ben trai, bo vien hop nang ne
    //  - Noi dung ngan gon, khong thuat ngu ky thuat noi bo, khong mat net chu.
    internal sealed class HopThoaiChinhSach : Form
    {
        public HopThoaiChinhSach()
        {
            Text = "Chính sách quyền riêng tư — Hệ thống tra cứu sức khỏe";
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.NenTrang;
            Font = AppTheme.ChuThuong;

            Rectangle vung = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
            ClientSize = new Size(
                Math.Min(this.Px(680), vung.Width - this.Px(60)),
                Math.Min(this.Px(610), vung.Height - this.Px(80)));

            int pad = this.Px(28);
            int w = ClientSize.Width - pad * 2;

            var dau = new Panel
            {
                Dock = DockStyle.Top,
                Height = this.Px(146),
                BackColor = AppTheme.NenTrang
            };
            dau.Paint += (_, e) =>
            {
                using var but = new Pen(AppTheme.Vien, 1f);
                e.Graphics.DrawLine(but, pad, dau.Height - 1, dau.Width - pad, dau.Height - 1);
            };

            dau.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.TieuDe,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Chính sách quyền riêng tư",
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(pad, this.Px(14), w, this.Px(46))
            });

            dau.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuThuong,
                ForeColor = AppTheme.ChuPhu,
                Text = "Tóm tắt ngắn gọn cách ứng dụng thu thập, sử dụng và bảo vệ thông tin cá nhân của bạn trong phạm vi đồ án sinh viên.",
                Bounds = new Rectangle(pad, this.Px(62), w, this.Px(40))
            });

            // 3 y dau chi dung dau cham tron, khong dong o/vien
            dau.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuNhoDam,
                ForeColor = AppTheme.Chinh,
                Text = "• Đồ án học tập sinh viên     • Chỉ phục vụ tra cứu tham khảo     • Không chia sẻ bên thứ ba",
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(pad, this.Px(106), w, this.Px(28))
            });

            var chan = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = this.Px(62),
                BackColor = AppTheme.NenTrang
            };
            chan.Paint += (_, e) =>
            {
                using var but = new Pen(AppTheme.Vien, 1f);
                e.Graphics.DrawLine(but, 0, 0, chan.Width, 0);
            };
            var btnDong = new FlatButton
            {
                Kieu = KieuNut.Chinh,
                BanKinhBo = 4,
                Text = "Đã hiểu",
                Bounds = new Rectangle(ClientSize.Width - pad - this.Px(124), this.Px(11), this.Px(124), this.Px(40))
            };
            btnDong.Click += (_, _) => Close();
            chan.Controls.Add(btnDong);

            var cuon = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.NenTrang,
                AutoScroll = true
            };

            int y = this.Px(18);
            int wNoiDung = w - this.Px(20);

            const string tomTatChu =
                "Hệ thống tra cứu sức khỏe là sản phẩm đồ án môn học do nhóm sinh viên phát triển. " +
                "Ứng dụng không mang mục đích thương mại, không chạy quảng cáo và không cung cấp thông tin cá nhân cho bên thứ ba.";
            Size szTomTat = TextRenderer.MeasureText(tomTatChu, AppTheme.ChuThuong,
                new Size(wNoiDung - this.Px(26), int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            int hTomTat = szTomTat.Height + this.Px(26);

            var tomTat = new Panel
            {
                BackColor = AppTheme.Nen,
                Bounds = new Rectangle(pad, y, wNoiDung, hTomTat)
            };
            tomTat.Paint += (_, e) =>
            {
                using var bTrai = new SolidBrush(AppTheme.Chinh);
                e.Graphics.FillRectangle(bTrai, 0, 0, this.Px(3), tomTat.Height);
            };
            tomTat.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuThuong,
                ForeColor = AppTheme.Chu,
                BackColor = AppTheme.Nen,
                Text = tomTatChu,
                Bounds = new Rectangle(this.Px(14), this.Px(10), wNoiDung - this.Px(22), szTomTat.Height + this.Px(14))
            });
            cuon.Controls.Add(tomTat);
            y += tomTat.Height + this.Px(20);

            (string TieuDe, string NoiDung)[] muc =
            {
                ("1. Dữ liệu chúng tôi thu thập",
                 "• Thông tin tài khoản: Họ và tên, tên đăng nhập, địa chỉ email (dùng để nhận diện tài khoản và hỗ trợ khôi phục mật khẩu).\n" +
                 "• Dữ liệu sử dụng: Từ khóa triệu chứng, bệnh lý hoặc tên thuốc bạn tìm kiếm cùng lịch sử tra cứu cá nhân trong ứng dụng."),

                ("2. Mục đích sử dụng dữ liệu",
                 "Thông tin chỉ được dùng để tạo và quản lý tài khoản, xác minh khi bạn cần đặt lại mật khẩu, lưu lịch sử tra cứu cá nhân và cải thiện trải nghiệm sử dụng ứng dụng."),

                ("3. Bảo mật và lưu trữ thông tin",
                 "• Mật khẩu của bạn được bảo vệ trước khi lưu trữ, không hiển thị hay lưu dưới dạng chữ thông thường.\n" +
                 "• Dữ liệu chỉ được lưu trong phạm vi vận hành của đồ án và không chuyển giao, mua bán hay chia sẻ cho bên thứ ba.\n" +
                 "• Thông tin có thể được xóa khi kết thúc quá trình học tập, đánh giá môn học hoặc khi bạn có yêu cầu."),

                ("4. Quyền và trách nhiệm của người dùng",
                 "• Bạn có quyền kiểm tra thông tin đã đăng ký, tắt tùy chọn ghi nhớ mật khẩu bất kỳ lúc nào, đổi mật khẩu hoặc yêu cầu xóa dữ liệu tài khoản.\n" +
                 "• Vui lòng tự bảo quản thông tin đăng nhập, không dùng mật khẩu trùng với các dịch vụ quan trọng khác và đăng xuất khi dùng máy chung."),

                ("5. Giới hạn bảo đảm & lưu ý y khoa",
                 "• Vì đây là đồ án do nhóm sinh viên thực hiện, nhóm nỗ lực bảo vệ thông tin trong khả năng cho phép nhưng không thể cam kết mức độ bảo mật tuyệt đối như các tổ chức, doanh nghiệp lớn. Khuyến nghị không nhập dữ liệu nhạy cảm ngoài phạm vi cần thiết.\n" +
                 "• Mọi nội dung về triệu chứng, bệnh lý và thuốc chỉ mang tính tham khảo, không thay thế chẩn đoán hay chỉ định của bác sĩ.")
            };

            foreach (var (td, nd) in muc)
            {
                var lblTd = new Label
                {
                    AutoSize = false,
                    UseMnemonic = false,
                    Font = AppTheme.ChuMuc,
                    ForeColor = AppTheme.TieuDeMau,
                    Text = td,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Bounds = new Rectangle(pad, y, wNoiDung, this.Px(30))
                };
                cuon.Controls.Add(lblTd);
                y += this.Px(32);

                Size sz = TextRenderer.MeasureText(nd, AppTheme.ChuThuong,
                    new Size(wNoiDung - this.Px(4), int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                int hNd = sz.Height + this.Px(18);
                var lblNd = new Label
                {
                    AutoSize = false,
                    UseMnemonic = false,
                    Font = AppTheme.ChuThuong,
                    ForeColor = AppTheme.ChuPhu,
                    Text = nd,
                    Bounds = new Rectangle(pad, y, wNoiDung, hNd)
                };
                cuon.Controls.Add(lblNd);
                y += hNd + this.Px(12);
            }

            cuon.AutoScrollMinSize = new Size(0, y + this.Px(14));

            Controls.Add(cuon);
            Controls.Add(dau);
            Controls.Add(chan);

            AcceptButton = btnDong;
            CancelButton = btnDong;
        }
    }
}
