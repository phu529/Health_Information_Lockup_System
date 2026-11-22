using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HealthClient.UI
{
    /// <summary>
    /// Lop chuyen trang (v25, lam lai v29): hieu ung doi giua form Dang nhap va Dang ky.
    ///
    ///  - Bang thuong hieu ben trai: hai anh hoa vao nhau (giu nguyen ngu canh, khong giat).
    ///  - Vung noi dung ben phai: noi dung cu truot nhe ra mot ben va mo dan, noi dung moi truot vao tu phia
    ///    nguoc lai va hien dan (lech pha de hai lop khong chong chu), kem mot dai sang chay ngang nhu phan chieu tren kinh.
    ///
    /// Chi dung HAI anh chup (tu, den) va mot control duy nhat, moi khung hinh chi la vai lenh DrawImage voi do trong
    /// nen nhe may; form that o duoi khong phai ve lai. Lop nay KHONG so huu hai anh (nguoi goi tu giai phong),
    /// khong co thuoc tinh public (tranh canh bao WFO1000), khong cham toi logic dang nhap.
    /// </summary>
    public sealed class LopChuyenTrang : Control
    {
        private readonly Bitmap _tu;
        private readonly Bitmap _den;
        private readonly Rectangle _vung;
        private readonly Color _nen;
        private readonly int _huong;
        private float _tienDo;

        /// <param name="tu">Anh man hinh luc dau.</param>
        /// <param name="den">Anh man hinh luc cuoi.</param>
        /// <param name="vungNoiDung">Vung ben phai (toa do client) se truot.</param>
        /// <param name="nen">Mau nen cua vung noi dung.</param>
        /// <param name="huong">+1: noi dung cu truot sang trai, moi vao tu phai (di tien). -1: nguoc lai (quay ve).</param>
        public LopChuyenTrang(Bitmap tu, Bitmap den, Rectangle vungNoiDung, Color nen, int huong)
        {
            _tu = tu;
            _den = den;
            _vung = vungNoiDung;
            _nen = nen;
            _huong = huong >= 0 ? 1 : -1;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Opaque, true);
            SetStyle(ControlStyles.Selectable, false);

            TabStop = false;
            Cursor = Cursors.Default;
        }

        /// <summary>t = 0..1 (da lam muot). Ve lai ngay.</summary>
        public void DatTienDo(float t)
        {
            _tienDo = Math.Clamp(t, 0f, 1f);
            Invalidate();
            Update();
        }

        // v38: khoi phuc ham tro giup bi mat (build bao CS0103 'DoTrong' o dong 79 va 104): ve anh voi do trong suot alpha.
        private static ImageAttributes DoTrong(float alpha)
        {
            var m = new ColorMatrix { Matrix33 = Math.Clamp(alpha, 0f, 1f) };
            var ia = new ImageAttributes();
            ia.SetColorMatrix(m, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            return ia;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 4 || Height < 4)
                return;

            Graphics g = e.Graphics;
            float px = DeviceDpi / 96f;
            float t = _tienDo;
            var toanBo = new Rectangle(0, 0, Width, Height);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            // 1) Ben ngoai vung noi dung (bang thuong hieu): anh cu hoa dan sang anh moi.
            g.SetClip(_vung, CombineMode.Exclude);
            g.DrawImage(_tu, toanBo);

            if (t > 0.004f)
            {
                using (ImageAttributes ia = DoTrong(t))
                    g.DrawImage(_den, toanBo, 0, 0, _den.Width, _den.Height, GraphicsUnit.Pixel, ia);
            }

            g.ResetClip();

            // 2) Vung noi dung (v29): "quet dien tim". Mot canh nghieng quet ngang vung noi dung; phia sau canh la
            //    noi dung moi (truot nhe vao), phia truoc canh la noi dung cu (lui nhe). Canh co vet sang xanh nhat
            //    va mot nhip dien tim vet lai phia sau, hop chu de y te.
            g.SetClip(_vung);

            using (var nen = new SolidBrush(_nen))
                g.FillRectangle(nen, _vung);

            float nghieng = 46f * px;
            float p = _huong > 0 ? t : 1f - t;
            float xe = _vung.X - nghieng + (_vung.Width + 2f * nghieng) * p;
            float xTren = xe + nghieng;
            float xDuoi = xe - nghieng;
            float truot = 38f * px;

            // Noi dung cu: lui nhe ve phia nguoc huong quet, toi dan.
            int dxCu = (int)Math.Round(-_huong * truot * t);
            var dichCu = new Rectangle(_vung.X + dxCu, _vung.Y, _vung.Width, _vung.Height);

            using (ImageAttributes ia = DoTrong(1f - 0.45f * t))
                g.DrawImage(_tu, dichCu, _vung.X, _vung.Y, _vung.Width, _vung.Height, GraphicsUnit.Pixel, ia);

            // Noi dung moi: chi hien o phia sau canh quet, truot vao tu phia quet toi.
            using (var vungMoi = new GraphicsPath())
            {
                float trai = _vung.X - 2f;
                float phai = _vung.Right + 2f;
                float tren = _vung.Y - 2f;
                float duoi = _vung.Bottom + 2f;

                if (_huong > 0)
                    vungMoi.AddPolygon(new[]
                    {
                        new PointF(trai, tren), new PointF(xTren, tren),
                        new PointF(xDuoi, duoi), new PointF(trai, duoi)
                    });
                else
                    vungMoi.AddPolygon(new[]
                    {
                        new PointF(xTren, tren), new PointF(phai, tren),
                        new PointF(phai, duoi), new PointF(xDuoi, duoi)
                    });

                g.SetClip(vungMoi, CombineMode.Intersect);

                int dxMoi = (int)Math.Round(_huong * truot * (1f - t));
                var dichMoi = new Rectangle(_vung.X + dxMoi, _vung.Y, _vung.Width, _vung.Height);
                g.DrawImage(_den, dichMoi, _vung.X, _vung.Y, _vung.Width, _vung.Height, GraphicsUnit.Pixel);
            }

            // 3) Canh quet phat sang + nhip dien tim phia sau canh.
            g.SetClip(_vung);

            if (t > 0.01f && t < 0.99f)
            {
                float dam = (float)Math.Sin(Math.PI * t);
                float y0 = _vung.Y - 2f;
                float y1 = _vung.Bottom + 2f;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingMode = CompositingMode.SourceOver;

                using (var p1 = new Pen(Color.FromArgb((int)(40f * dam), 150, 226, 255), 26f * px))
                using (var p2 = new Pen(Color.FromArgb((int)(96f * dam), 190, 236, 255), 9f * px))
                using (var p3 = new Pen(Color.FromArgb((int)(235f * dam), 238, 251, 255), 2f * px))
                {
                    g.DrawLine(p1, xTren, y0, xDuoi, y1);
                    g.DrawLine(p2, xTren, y0, xDuoi, y1);
                    g.DrawLine(p3, xTren, y0, xDuoi, y1);
                }

                // Nhip dien tim: vet lai phia sau canh, o giua chieu cao. u = 1 la sat canh quet.
                float[,] nhip =
                {
                    { 0f, 0f }, { 0.50f, 0f }, { 0.55f, -0.08f }, { 0.60f, 0f }, { 0.65f, 0.10f },
                    { 0.70f, -0.55f }, { 0.75f, 0.30f }, { 0.80f, 0f }, { 0.88f, -0.14f }, { 0.94f, 0f }, { 1f, 0f }
                };

                float dai = 150f * px;
                float bien = 34f * px;
                float ym = _vung.Y + _vung.Height / 2f;
                var diem = new PointF[nhip.GetLength(0)];

                for (int i = 0; i < diem.Length; i++)
                {
                    float u = nhip[i, 0];
                    float x = _huong > 0 ? xe - dai * (1f - u) : xe + dai * (1f - u);
                    diem[i] = new PointF(x, ym + nhip[i, 1] * bien);
                }

                using (var but = new Pen(Color.FromArgb((int)(225f * dam), 106, 213, 255), 2.2f * px))
                {
                    but.LineJoin = LineJoin.Round;
                    but.StartCap = LineCap.Round;
                    but.EndCap = LineCap.Round;
                    g.DrawLines(but, diem);
                }
            }

            g.ResetClip();
        }
    }
}
