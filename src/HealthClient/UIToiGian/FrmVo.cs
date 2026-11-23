using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Cua so duy nhat cho 3 man hinh: Khoi dau, Dang nhap, Dang ky.
    // Chan trang: DongCanhBaoTinh dung chung — chu canh bao dang TINH (khong nhap nhay, khong vien hong).
    internal sealed class FrmVo : Form
    {
        private readonly Panel _noiDung = new();
        private readonly DongCanhBaoTinh _chanTrang = new();
        private ITrang? _hienTai;
        private int _thuTuTrang;
        private int _tabXacThucCu = -1;

        public FrmVo()
        {
            Text = "Hệ thống tra cứu sức khỏe";
            AutoScaleMode = AutoScaleMode.None;
            Font = AppTheme.ChuThuong;
            BackColor = AppTheme.Nen;
            StartPosition = FormStartPosition.CenterScreen;

            Rectangle vung = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
            ClientSize = new Size(
                Math.Min(this.Px(1040), Math.Max(this.Px(880), vung.Width - this.Px(40))),
                Math.Min(this.Px(680), Math.Max(this.Px(600), vung.Height - this.Px(48))));
            MinimumSize = new Size(
                Math.Min(this.Px(880), ClientSize.Width),
                Math.Min(this.Px(595), ClientSize.Height));

            _noiDung.Dock = DockStyle.Fill;
            _noiDung.BackColor = AppTheme.Nen;

            Controls.Add(_noiDung);
            Controls.Add(_chanTrang);

            HienKhoiDau();
        }

        public void HienKhoiDau(int tab = 0)
        {
            _tabXacThucCu = -1;
            if (_hienTai is TrangKhoiDau kd)
            {
                kd.ChonTab(tab);
                AcceptButton = kd.NutMacDinh;
                kd.OBatDau?.Focus();
                return;
            }
            Chuyen(new TrangKhoiDau(this, tab), 0);
        }

        public void HienDangNhap(string? nhacNho = null, string? tenGoiY = null)
        {
            int tabTruoc = _tabXacThucCu == 1 ? 1 : -1;
            _tabXacThucCu = 0;
            Chuyen(new TrangDangNhap(this, nhacNho, tenGoiY, tabTruoc), 1);
        }

        public void HienDangKy()
        {
            int tabTruoc = _tabXacThucCu == 0 ? 0 : -1;
            _tabXacThucCu = 1;
            Chuyen(new TrangDangKy(this, tabTruoc), 2);
        }

        public void MoTrangChu(string hoTen)
        {
            Hide();
            try
            {
                using var frm = new FrmTrangChu(hoTen);
                frm.ShowDialog();
            }
            finally
            {
                _tabXacThucCu = -1;
                Show();
                HienDangNhap();
            }
        }

        private void Chuyen(UserControl trang, int thuTuMoi)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(() => ChuyenNgay(trang, thuTuMoi)));
            else
                ChuyenNgay(trang, thuTuMoi);
        }

        private void ChuyenNgay(UserControl trang, int thuTuMoi)
        {
            if (IsDisposed)
            {
                trang.Dispose();
                return;
            }

            int huong = thuTuMoi >= _thuTuTrang ? 1 : -1;
            _thuTuTrang = thuTuMoi;

            void Doi()
            {
                Control? cu = _noiDung.Controls.Count > 0 ? _noiDung.Controls[0] : null;
                trang.Dock = DockStyle.Fill;
                _noiDung.Controls.Add(trang);
                if (cu != null)
                {
                    _noiDung.Controls.Remove(cu);
                    cu.Dispose();
                }
            }

            if (_noiDung.Controls.Count == 0 || !Visible)
            {
                Doi();
                (trang as TrangXacThuc)?.HoanTatChuyenMan();
            }
            else
            {
                HieuUngChuyen.Chay(_noiDung, Doi, huong, () => (trang as TrangXacThuc)?.HoanTatChuyenMan());
            }

            _hienTai = trang as ITrang;
            AcceptButton = _hienTai?.NutMacDinh;
            _hienTai?.OBatDau?.Focus();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _hienTai?.OBatDau?.Focus();
        }

        // Canh bao y khoa dang TINH (khong nhap nhay, khong vien hong).
        private sealed class DongCanhBaoTinh : Control
        {
            private const string NoiDung =
                "Thông tin chỉ mang tính tham khảo, không thể thay thế chẩn đoán của bác sĩ, " +
                "nếu vấn đề của bạn quan trọng hãy đến các bệnh viện gần nhất.";

            private const TextFormatFlags CoChu =
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

            private bool _dangDo;

            public DongCanhBaoTinh()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.Selectable, false);
                TabStop = false;
                Dock = DockStyle.Bottom;
                Height = 40;
                BackColor = AppTheme.NenTrang;
                Font = AppTheme.ChuCanhBao;
                AccessibleRole = AccessibleRole.Alert;
                AccessibleName = "Cảnh báo y khoa";
            }

            private void CapNhatCao()
            {
                if (_dangDo || Width <= 0) return;
                int szIc = this.Px(16);
                int wChu = Math.Max(this.Px(140), Width - this.Px(64) - szIc);
                Size sz = TextRenderer.MeasureText(NoiDung, Font, new Size(wChu, int.MaxValue), CoChu);
                int caoMoi = Math.Max(this.Px(40), sz.Height + this.Px(16));
                if (Height != caoMoi)
                {
                    _dangDo = true;
                    try { Height = caoMoi; }
                    finally { _dangDo = false; }
                }
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                CapNhatCao();
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                CapNhatCao();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BackColor);

                using (var duongTren = new Pen(AppTheme.Vien, 1f))
                    g.DrawLine(duongTren, 0, 0, Width, 0);

                Color mauChu = AppTheme.CanhBaoDo1;
                int szIc = this.Px(16);
                int gapIc = this.Px(8);
                int wToiDa = Math.Max(this.Px(140), Width - this.Px(48) - szIc - gapIc);
                Size sz = TextRenderer.MeasureText(g, NoiDung, Font, new Size(wToiDa, int.MaxValue), CoChu);

                int wBlock = Math.Min(wToiDa, sz.Width);
                int tong = szIc + gapIc + wBlock;
                int xBlock = Math.Max(this.Px(16), (Width - tong) / 2);
                int yBlock = this.Px(4);
                int hBlock = Height - this.Px(8);

                LucideIcons.Draw(g, LucideIconKind.TriangleAlert,
                    new RectangleF(xBlock, yBlock + (hBlock - szIc) / 2f, szIc, szIc), mauChu, 1.9f);

                var vungChu = new Rectangle(xBlock + szIc + gapIc, yBlock, wBlock + this.Px(4), hBlock);
                TextRenderer.DrawText(g, NoiDung, Font, vungChu, mauChu, CoChu);
            }
        }
    }
}
