using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Danh sach cau hoi thuong gap Ban 9:
    //  - Loc thong minh theo tu ngu (khop dau tu & khong dau): nhap "T" loc ngay cac cau bat dau/co tu bat dau bang T,
    //    nhap cang cu the loc cang chinh xac va sap xep do phu hop cao nhat len dau.
    internal sealed class FaqAccordion : Control
    {
        private const int ThoiGianMoMs = 170;
        private static readonly char[] TachTu = { ' ', ',', '.', ';', ':', '?', '!', '-', '/', '(', ')', '"' };

        private readonly (string Nhom, string Hoi, string Dap)[] _mucGoc;
        private readonly string[] _hoiKhongDau;
        private readonly string[] _dapKhongDau;
        private readonly string[][] _tuHoiKhongDau;
        private readonly string[][] _tuDapKhongDau;

        private readonly List<int> _hienThi = new();
        private readonly float[] _tuMo;
        private readonly float[] _mo;
        private readonly float[] _dich;
        private readonly int[] _caoDapCache;

        private readonly Stopwatch _sw = new();
        private readonly System.Windows.Forms.Timer _nhip = new() { Interval = 16 };
        private int _hover = -1;
        private string _nhomHienTai = string.Empty;
        private string _tuKhoaHienTai = string.Empty;

        public event EventHandler? ChieuCaoDoi;

        public FaqAccordion((string Nhom, string Hoi, string Dap)[] muc)
        {
            _mucGoc = muc;
            _hoiKhongDau = new string[muc.Length];
            _dapKhongDau = new string[muc.Length];
            _tuHoiKhongDau = new string[muc.Length][];
            _tuDapKhongDau = new string[muc.Length][];

            _tuMo = new float[muc.Length];
            _mo = new float[muc.Length];
            _dich = new float[muc.Length];
            _caoDapCache = new int[muc.Length];

            for (int i = 0; i < muc.Length; i++)
            {
                _hoiKhongDau[i] = BoDau(muc[i].Hoi);
                _dapKhongDau[i] = BoDau(muc[i].Dap);
                _tuHoiKhongDau[i] = _hoiKhongDau[i].Split(TachTu, StringSplitOptions.RemoveEmptyEntries);
                _tuDapKhongDau[i] = _dapKhongDau[i].Split(TachTu, StringSplitOptions.RemoveEmptyEntries);
                _hienThi.Add(i);
            }

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            TabStop = false;
            Cursor = Cursors.Hand;
            Font = AppTheme.ChuDocDam;

            CapNhatCacheDapAn();
            _nhip.Tick += (_, _) => Nhip();
        }

        private static string BoDau(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            string norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (char c in norm)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(c switch
                {
                    'đ' or 'Đ' => 'd',
                    _ => char.ToLowerInvariant(c)
                });
            }
            return sb.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nhip.Stop();
                _nhip.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible && _nhip.Enabled)
            {
                _nhip.Stop();
                for (int i = 0; i < _mo.Length; i++) _mo[i] = _dich[i];
                CapNhatChieuCao();
            }
        }

        private int CaoTieuDe => this.Px(64);
        private int Khoang => this.Px(12);
        private int LeNgang => this.Px(22);

        private void CapNhatCacheDapAn()
        {
            int w = Math.Max(40, Width - LeNgang * 2 - this.Px(16));
            for (int i = 0; i < _mucGoc.Length; i++)
            {
                int h = ChuThoang.DoCao(_mucGoc[i].Dap, AppTheme.ChuDoc, w, this.Px(7), this.Px(10)) + this.Px(30);
                _caoDapCache[i] = h;
            }
        }

        private int CaoMuc(int idxGoc) => CaoTieuDe + (int)Math.Round(_caoDapCache[idxGoc] * _mo[idxGoc]);

        public int ChieuCaoCanThiet()
        {
            if (_hienThi.Count == 0) return this.Px(68);
            int tong = 0;
            for (int k = 0; k < _hienThi.Count; k++)
            {
                tong += CaoMuc(_hienThi[k]) + (k < _hienThi.Count - 1 ? Khoang : 0);
            }
            return tong + this.Px(2);
        }

        private void CapNhatChieuCao()
        {
            int cao = ChieuCaoCanThiet();
            if (Height != cao)
            {
                Height = cao;
                ChieuCaoDoi?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Loc(string? tuKhoa)
        {
            _tuKhoaHienTai = (tuKhoa ?? string.Empty).Trim();
            ApDungLoc();
        }

        public void LocTheoNhom(string? nhom)
        {
            _nhomHienTai = (nhom ?? string.Empty).Trim();
            ApDungLoc();
        }

        // Loc theo tu ngu:
        //  - 1 ky tu (vd "T"): chi khop cau hoi co tu BAT DAU bang "t" (uu tien cau bat dau bang "t")
        //  - Nhieu ky tu / nhieu tu (vd "tra cuu thuoc"): moi tu phai khop dau tu trong cau hoi hoac dap an,
        //    uu tien cao nhat cho khop chinh xac cum tu trong tieu de cau hoi.
        private void ApDungLoc()
        {
            _hienThi.Clear();
            string qNorm = BoDau(_tuKhoaHienTai);
            string[] tokens = qNorm.Split(TachTu, StringSplitOptions.RemoveEmptyEntries);

            var ketQua = new List<(int Idx, int Diem)>();

            for (int i = 0; i < _mucGoc.Length; i++)
            {
                if (_nhomHienTai.Length > 0 &&
                    !_mucGoc[i].Nhom.Equals(_nhomHienTai, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (tokens.Length == 0)
                {
                    ketQua.Add((i, 0));
                    continue;
                }

                int diem = TinhDiemKhop(i, qNorm, tokens);
                if (diem > 0)
                    ketQua.Add((i, diem));
            }

            if (tokens.Length > 0)
                ketQua.Sort((a, b) => b.Diem != a.Diem ? b.Diem.CompareTo(a.Diem) : a.Idx.CompareTo(b.Idx));

            foreach (var item in ketQua)
                _hienThi.Add(item.Idx);

            _hover = -1;
            CapNhatChieuCao();
            Invalidate();
        }

        private int TinhDiemKhop(int idx, string qNorm, string[] tokens)
        {
            string hoi = _hoiKhongDau[idx];
            string dap = _dapKhongDau[idx];
            string[] tuHoi = _tuHoiKhongDau[idx];
            string[] tuDap = _tuDapKhongDau[idx];

            // Neu chi nhap 1 ky tu don (vd "T"), chi loc cac cau hoi co tu bat dau bang ky tu do de thu hep ro ret
            if (tokens.Length == 1 && tokens[0].Length == 1)
            {
                string c = tokens[0];
                if (hoi.StartsWith(c, StringComparison.Ordinal)) return 100;
                if (tuHoi.Any(w => w.StartsWith(c, StringComparison.Ordinal))) return 60;
                return 0;
            }

            int diem = 0;
            bool khopTatCa = true;

            foreach (string tok in tokens)
            {
                bool khopDauTuHoi = tuHoi.Any(w => w.StartsWith(tok, StringComparison.Ordinal));
                bool khopChuoiHoi = !khopDauTuHoi && tok.Length >= 2 && hoi.Contains(tok, StringComparison.Ordinal);
                bool khopDauTuDap = tuDap.Any(w => w.StartsWith(tok, StringComparison.Ordinal));
                bool khopChuoiDap = !khopDauTuDap && tok.Length >= 3 && dap.Contains(tok, StringComparison.Ordinal);

                if (khopDauTuHoi) diem += 30;
                else if (khopChuoiHoi) diem += 18;
                else if (khopDauTuDap) diem += 10;
                else if (khopChuoiDap) diem += 5;
                else
                {
                    khopTatCa = false;
                    break;
                }
            }

            if (!khopTatCa) return 0;

            if (hoi.StartsWith(qNorm, StringComparison.Ordinal)) diem += 60;
            else if (hoi.Contains(qNorm, StringComparison.Ordinal)) diem += 35;
            else if (dap.Contains(qNorm, StringComparison.Ordinal)) diem += 12;

            return diem;
        }

        public void MoMuc(int idxGoc)
        {
            for (int k = 0; k < _dich.Length; k++)
            {
                _tuMo[k] = _mo[k];
                _dich[k] = k == idxGoc && _dich[k] < 0.5f ? 1f : 0f;
            }

            if (Visible && IsHandleCreated && SystemInformation.UIEffectsEnabled)
            {
                _sw.Restart();
                _nhip.Start();
            }
            else
            {
                _nhip.Stop();
                for (int k = 0; k < _mo.Length; k++) _mo[k] = _dich[k];
                CapNhatChieuCao();
                Invalidate();
            }
        }

        private void Nhip()
        {
            if (!Visible)
            {
                _nhip.Stop();
                for (int k = 0; k < _mo.Length; k++) _mo[k] = _dich[k];
                return;
            }

            float t = Math.Clamp(_sw.ElapsedMilliseconds / (float)ThoiGianMoMs, 0f, 1f);
            float e = AppTheme.EaseOutCubic(t);
            for (int i = 0; i < _mo.Length; i++)
                _mo[i] = _tuMo[i] + (_dich[i] - _tuMo[i]) * e;

            if (t >= 1f)
            {
                for (int i = 0; i < _mo.Length; i++) _mo[i] = _dich[i];
                _nhip.Stop();
            }

            CapNhatChieuCao();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CapNhatCacheDapAn();
            CapNhatChieuCao();
        }

        private int MucTai(Point p)
        {
            int y = 0;
            for (int k = 0; k < _hienThi.Count; k++)
            {
                int idx = _hienThi[k];
                int cao = CaoMuc(idx);
                if (p.Y >= y && p.Y < y + cao) return idx;
                y += cao + Khoang;
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = MucTai(e.Location);
            if (i != _hover)
            {
                _hover = i;
                Cursor = i >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != -1) { _hover = -1; Invalidate(); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int i = MucTai(e.Location);
            if (i >= 0) MoMuc(i);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.Nen);

            if (_hienThi.Count == 0)
            {
                var rEmpty = new RectangleF(1f, 1f, Width - 3f, Height - 3f);
                using var p = AppTheme.DuongBoGoc(rEmpty, this.Px(8));
                using var bg = new SolidBrush(AppTheme.NenTrang);
                using var pen = new Pen(AppTheme.Vien, 1f);
                g.FillPath(bg, p);
                g.DrawPath(pen, p);
                TextRenderer.DrawText(g, "Không tìm thấy câu hỏi phù hợp với từ khóa của bạn.",
                    AppTheme.ChuDoc, ClientRectangle, AppTheme.ChuPhu,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                return;
            }

            int y = 0;
            foreach (int i in _hienThi)
            {
                int cao = CaoMuc(i);
                var r = new RectangleF(1f, y + 1f, Width - 3f, cao - 2f);
                bool dangMo = _mo[i] > 0.02f;
                bool dangHover = _hover == i;
                Color nenThe = dangHover && !dangMo ? AppTheme.NenHover : AppTheme.NenTrang;

                using (var p = AppTheme.DuongBoGoc(r, this.Px(8)))
                using (var nen = new SolidBrush(nenThe))
                using (var vien = new Pen(dangMo ? AppTheme.ChinhSang : dangHover ? AppTheme.VienDam : AppTheme.Vien,
                           dangMo ? 1.5f : 1f))
                {
                    g.FillPath(nen, p);

                    if (dangMo)
                    {
                        var st = g.Save();
                        g.SetClip(p, CombineMode.Intersect);
                        using (var bgHead = new SolidBrush(Color.FromArgb(247, 250, 255)))
                            g.FillRectangle(bgHead, 0, y, Width, CaoTieuDe);
                        using (var duong = new Pen(AppTheme.Vien))
                            g.DrawLine(duong, LeNgang, y + CaoTieuDe, Width - LeNgang, y + CaoTieuDe);

                        float fade = Math.Clamp(_mo[i] / 0.65f, 0f, 1f);
                        Color mauDap = AppTheme.Tron(AppTheme.NenTrang, AppTheme.Chu, fade);
                        ChuThoang.Ve(g, _mucGoc[i].Dap, AppTheme.ChuDoc,
                            new Rectangle(LeNgang, y + CaoTieuDe + this.Px(14),
                                Width - LeNgang * 2 - this.Px(16), _caoDapCache[i]),
                            mauDap, this.Px(7), this.Px(10));
                        g.Restore(st);
                    }

                    g.DrawPath(vien, p);
                }

                if (dangMo)
                {
                    using var nhan = new SolidBrush(AppTheme.ChinhSang);
                    g.FillRectangle(nhan, 1f, y + this.Px(12), Math.Max(3f, this.Px(3)), CaoTieuDe - this.Px(24));
                }

                TextRenderer.DrawText(g, _mucGoc[i].Hoi, Font,
                    new Rectangle(LeNgang, y + this.Px(4), Width - LeNgang - this.Px(56), CaoTieuDe - this.Px(8)),
                    dangMo ? AppTheme.Chinh : AppTheme.TieuDeMau,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak |
                    TextFormatFlags.NoPrefix);

                float cx = Width - this.Px(30);
                float cy = y + CaoTieuDe / 2f;
                float dy = this.Px(3) * (1f - 2f * _mo[i]);
                float dx = this.Px(6);
                using (var chev = new Pen(dangMo ? AppTheme.ChinhSang : AppTheme.ChuPhu, 1.9f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                    g.DrawLines(chev, new[]
                    {
                        new PointF(cx - dx, cy - dy), new PointF(cx, cy + dy), new PointF(cx + dx, cy - dy)
                    });

                y += cao + Khoang;
            }
        }
    }
}
