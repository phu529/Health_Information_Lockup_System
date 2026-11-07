using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UI
{
    /// <summary>
    /// Lop chuyen canh (v22): hieu ung "mo ra" tu diem nguoi dung bam chuot. Mot vong tron no dan tu diem bam,
    /// ben trong lo ra form Dang nhap / Dang ky (thu nho nhe tu 1,04 ve 1,00), ben ngoai la bia chao
    /// phong nhe ra (1,00 den 1,05, quanh diem bam) va toi dan. Hai chuyen dong nguoc chieu nhau tao do sau.
    /// Mep vong co duong sang xanh nhat, kem mot vong vang anh phia sau.
    ///
    /// Cach lam de muot va nhe may: chi co HAI anh chup (bia chao va form), ve bang mot control duy nhat.
    /// Moi khung hinh chi la hai lenh DrawImage co cat vung; khong dung lai cac control that ben duoi,
    /// nen form that khong phai ve lai trong luc chay. Xong hieu ung thi hai anh duoc giai phong ngay
    /// (khoang 5 MB cho cua so 920 x 680), control nay bi go khoi form.
    ///
    /// Lop nay khong co thuoc tinh public (tranh canh bao WFO1000), khong cham toi logic dang nhap.
    /// </summary>
    public sealed class LopChuyenCanh : Control
    {
        private Bitmap? _bia;
        private Bitmap? _form;
        private readonly Point _tam;
        private float _tienDo;

        public LopChuyenCanh(Bitmap bia, Bitmap form, Point tam)
        {
            _bia = bia;
            _form = form;
            _tam = tam;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Opaque, true);
            SetStyle(ControlStyles.Selectable, false);

            TabStop = false;
            Cursor = Cursors.Default;
        }

        /// <summary>t = 0..1 (da lam muot). Ve lai ngay, khong doi hang doi thong diep.</summary>
        public void DatTienDo(float t)
        {
            _tienDo = Math.Clamp(t, 0f, 1f);
            Invalidate();
            Update();
        }

        /// <summary>Dat tien do ban dau KHONG ve (dung truoc khi them vao form, de khung dau tien da dung).</summary>
        public void DatTienDoKhongVe(float t)
        {
            _tienDo = Math.Clamp(t, 0f, 1f);
        }

        // Ban kinh du de vong tron tu diem bam phu kin ca bon goc cua so.
        private float BanKinhToiDa()
        {
            float xa = Math.Max(_tam.X, Width - _tam.X);
            float ya = Math.Max(_tam.Y, Height - _tam.Y);
            return (float)Math.Sqrt(xa * xa + ya * ya) + 2f;
        }

        // Ve anh phong theo he so "he" quanh diem bam. he <= 1 thi ve vua khit, khong phong.
        private void VeAnh(Graphics g, Bitmap anh, float he, int w, int h)
        {
            if (he <= 1.0005f)
            {
                g.DrawImage(anh, new Rectangle(0, 0, w, h));
                return;
            }

            var dich = new RectangleF(_tam.X * (1f - he), _tam.Y * (1f - he), w * he, h * he);
            g.DrawImage(anh, dich, new RectangleF(0, 0, w, h), GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Bitmap? bia = _bia;
            Bitmap? form = _form;

            if (bia == null || form == null || Width < 4 || Height < 4)
                return;

            Graphics g = e.Graphics;
            int w = Width;
            int h = Height;
            float t = _tienDo;
            float r = BanKinhToiDa() * t;
            float px = DeviceDpi / 96f;
            var toanBo = new Rectangle(0, 0, w, h);

            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.CompositingMode = CompositingMode.SourceCopy;

            using (var vong = new GraphicsPath())
            {
                bool coVong = r >= 1f;

                if (coVong)
                    vong.AddEllipse(_tam.X - r, _tam.Y - r, r * 2f, r * 2f);

                // 1) Ben trong vong tron: form Dang nhap, thu nho nhe tu 1,04 ve 1,00.
                if (coVong)
                {
                    g.SetClip(vong);
                    VeAnh(g, form, 1.04f - 0.04f * t, w, h);
                }

                // 2) Ben ngoai vong tron: bia chao, phong nhe ra va toi dan de form noi bat hon.
                g.ResetClip();

                if (coVong)
                    g.SetClip(vong, CombineMode.Exclude);

                VeAnh(g, bia, 1f + 0.05f * t, w, h);

                g.CompositingMode = CompositingMode.SourceOver;

                int toi = (int)(110f * t);

                if (toi > 0)
                {
                    using (var phu = new SolidBrush(Color.FromArgb(toi, 4, 16, 64)))
                        g.FillRectangle(phu, toanBo);
                }

                g.ResetClip();

                // 3) Mep vong: duong sang + quang mo + vong vang anh. Ve sau cung nen che luon rang cua cua vung cat.
                if (coVong && t < 0.98f)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    int a = (int)(235f * (1f - t * t));
                    float quang = 10f * px;

                    using (var pen1 = new Pen(Color.FromArgb(a / 4, 150, 226, 255), quang))
                    using (var pen2 = new Pen(Color.FromArgb(a, 210, 242, 255), 2.4f * px))
                    {
                        g.DrawEllipse(pen1, _tam.X - r, _tam.Y - r, r * 2f, r * 2f);
                        g.DrawEllipse(pen2, _tam.X - r, _tam.Y - r, r * 2f, r * 2f);
                    }

                    float r2 = r * 0.88f;

                    if (r2 > 8f)
                    {
                        using (var pen3 = new Pen(Color.FromArgb(a / 5, 190, 236, 255), 1.4f * px))
                            g.DrawEllipse(pen3, _tam.X - r2, _tam.Y - r2, r2 * 2f, r2 * 2f);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bia?.Dispose();
                _form?.Dispose();
                _bia = null;
                _form = null;
            }

            base.Dispose(disposing);
        }
    }
}
