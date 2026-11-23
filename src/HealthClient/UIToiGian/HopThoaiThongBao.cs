using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Hop thoai thong bao toi gian, trang trong (thay the MessageBox mac dinh cua Windows)
    internal sealed class HopThoaiThongBao : Form
    {
        private readonly Color _mauNhan;
        private readonly Color _mauNenIcon;
        private readonly LucideIconKind _icon;

        private HopThoaiThongBao(
            string tieuDe,
            string noiDung,
            string chuNut,
            Color mauNhan,
            Color mauNenIcon,
            LucideIconKind icon)
        {
            _mauNhan = mauNhan;
            _mauNenIcon = mauNenIcon;
            _icon = icon;

            Text = tieuDe;
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = AppTheme.NenTrang;
            Font = AppTheme.ChuThuong;
            DoubleBuffered = true;

            int w = this.Px(430);
            int pad = this.Px(28);
            int wNoiDung = w - pad * 2;

            var lblTd = new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuMucLon,
                ForeColor = AppTheme.TieuDeMau,
                BackColor = AppTheme.NenTrang,
                Text = tieuDe,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(pad, this.Px(96), wNoiDung, this.Px(34))
            };

            int hNd = ChuThoang.DoCao(noiDung, AppTheme.ChuDoc, wNoiDung, this.Px(6), this.Px(8));
            var lblNd = new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuDoc,
                ForeColor = AppTheme.ChuPhu,
                BackColor = AppTheme.NenTrang,
                Text = noiDung,
                TextAlign = ContentAlignment.TopCenter,
                Bounds = new Rectangle(pad, this.Px(134), wNoiDung, Math.Max(this.Px(44), hNd + this.Px(8)))
            };

            int yNut = lblNd.Bottom + this.Px(20);
            int wNut = this.Px(210);
            var btnOk = new FlatButton
            {
                Kieu = KieuNut.Chinh,
                BanKinhBo = 6,
                Text = chuNut,
                Font = AppTheme.ChuDocDam,
                Bounds = new Rectangle((w - wNut) / 2, yNut, wNut, this.Px(44))
            };
            btnOk.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };

            ClientSize = new Size(w, btnOk.Bottom + this.Px(26));
            Controls.AddRange(new Control[] { lblTd, lblNd, btnOk });

            AcceptButton = btnOk;
            CancelButton = btnOk;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Dai mau nhan trang trong o canh tren (4px)
            using (var bDai = new SolidBrush(_mauNhan))
                g.FillRectangle(bDai, 0, 0, Width, this.Px(4));

            // Vien khung sach se
            using (var pVien = new Pen(AppTheme.VienO, 1.2f))
                g.DrawRectangle(pVien, 0.5f, 0.5f, Width - 1.5f, Height - 1.5f);

            // Vong tron hao quang mem + bieu tuong chinh giua
            int dNgoai = this.Px(58);
            int dTrong = this.Px(44);
            float cx = Width / 2f;
            float cy = this.Px(52);

            using (var bNgoai = new SolidBrush(Color.FromArgb(145, _mauNenIcon)))
                g.FillEllipse(bNgoai, cx - dNgoai / 2f, cy - dNgoai / 2f, dNgoai, dNgoai);
            using (var bTrong = new SolidBrush(_mauNenIcon))
                g.FillEllipse(bTrong, cx - dTrong / 2f, cy - dTrong / 2f, dTrong, dTrong);
            using (var pVong = new Pen(Color.FromArgb(120, _mauNhan), 1.4f))
                g.DrawEllipse(pVong, cx - dTrong / 2f, cy - dTrong / 2f, dTrong, dTrong);

            int szIc = this.Px(22);
            LucideIcons.Draw(g, _icon,
                new RectangleF(cx - szIc / 2f, cy - szIc / 2f, szIc, szIc), _mauNhan, 2.3f);
        }

        public static void HienThanhCong(IWin32Window? owner, string tieuDe, string noiDung, string chuNut = "Tiếp tục")
        {
            using var dlg = new HopThoaiThongBao(
                tieuDe,
                noiDung,
                chuNut,
                AppTheme.Phu,
                AppTheme.ChipMint,
                LucideIconKind.Check);
            if (owner != null) dlg.ShowDialog(owner);
            else dlg.ShowDialog();
        }
    }
}
