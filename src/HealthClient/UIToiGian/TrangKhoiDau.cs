using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HealthClient.UIToiGian
{
    // Man hinh mo dau Ban 9:
    //  - Tab Gioi thieu: co dong chu gon gang, nen xanh nhat (#F8FAFC) de cac the trang vien ro + thanh nhan Navy/Teal noi bat can bang,
    //    bo sung icon truc quan dong nhat (khong mau me), BO khoi luu y canh bao trung lap o tren chan trang, sua khuyet chu "quy" o chan trang.
    internal sealed class TrangKhoiDau : UserControl, ITrang
    {
        private readonly FrmVo _shell;
        private readonly Panel _dau = new();
        private readonly Logo _logoDau = new();
        private readonly Label _tenDau = new();
        private readonly TabSwitch _nav = new("Trang chủ", "Giới thiệu", "Hỗ trợ");
        private readonly FlatButton _nutDangKy = new();
        private readonly FlatButton _nutDangNhap = new();

        private readonly Panel _than = new();
        private readonly Panel _tabTrangChu = new();
        private readonly Panel _tabGioiThieu = new();
        private readonly Panel _tabHoTro = new();

        private readonly Panel _khungTrangChu = new();
        private readonly Panel _traiTrangChu = new();
        private readonly Panel _minhHoa = new();
        private SloganBar? _sloganBar;
        private readonly FlatButton _nutBatDau = new();
        private readonly Panel[] _tronMuc = new Panel[3];
        private readonly float[] _hoverMucT = new float[3];
        private readonly float[] _hoverMucTu = new float[3];
        private readonly int[] _hoverMucDich = new int[3];
        private readonly Stopwatch _swHoverMuc = new();
        private readonly System.Windows.Forms.Timer _nhipHoverMuc = new() { Interval = 16 };

        private Panel? _khungHoTro;
        private FormField? _oTimFaq;
        private FaqAccordion? _faq;
        private Panel? _hopHoTro;
        private readonly List<(FlatButton Nut, string Nhom)> _nutNhomFaq = new();
        private string _nhomFaqDangChon = string.Empty;
        private int _tabHienTai = -1;

        public IButtonControl? NutMacDinh => _nav.ChiSo == 0 ? _nutBatDau : null;
        public FormField? OBatDau => null;

        public TrangKhoiDau(FrmVo shell, int tabMacDau = 0)
        {
            _shell = shell;
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Nen;

            TaoThanhDau(tabMacDau);

            _than.Dock = DockStyle.Fill;
            _than.BackColor = AppTheme.Nen;
            _than.AutoScroll = true;

            TaoTabTrangChu();
            TaoTabGioiThieu();
            TaoTabHoTro();

            _than.Controls.Add(_tabTrangChu);
            _than.Controls.Add(_tabGioiThieu);
            _than.Controls.Add(_tabHoTro);
            _than.Resize += (_, _) => CanGiuaNoiDung();

            Controls.Add(_than);
            Controls.Add(_dau);

            _nhipHoverMuc.Tick += (_, _) =>
            {
                if (!Visible)
                {
                    _nhipHoverMuc.Stop();
                    return;
                }
                float t = Math.Clamp(_swHoverMuc.ElapsedMilliseconds / 120f, 0f, 1f);
                float k = AppTheme.EaseOutCubic(t);
                for (int i = 0; i < 3; i++)
                {
                    _hoverMucT[i] = _hoverMucTu[i] + (_hoverMucDich[i] - _hoverMucTu[i]) * k;
                    _tronMuc[i]?.Invalidate();
                }
                if (t >= 1f)
                {
                    for (int i = 0; i < 3; i++) _hoverMucT[i] = _hoverMucDich[i];
                    _nhipHoverMuc.Stop();
                }
            };

            ChonTab(tabMacDau);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nhipHoverMuc.Stop();
                _nhipHoverMuc.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible) _nhipHoverMuc.Stop();
        }

        private void TaoThanhDau(int tabMacDau)
        {
            _dau.Dock = DockStyle.Top;
            _dau.Height = this.Px(68);
            _dau.BackColor = AppTheme.NenTrang;
            _dau.Paint += (_, e) =>
            {
                using var but = new Pen(AppTheme.Vien);
                e.Graphics.DrawLine(but, 0, _dau.Height - 1, _dau.Width, _dau.Height - 1);
            };

            _logoDau.SetBounds(this.Px(28), this.Px(18), this.Px(32), this.Px(32));
            _logoDau.Click += (_, _) => ChonTab(0);

            _tenDau.AutoSize = false;
            _tenDau.UseMnemonic = false;
            _tenDau.Text = "Hệ thống tra cứu sức khỏe";
            _tenDau.Font = AppTheme.ChuMuc;
            _tenDau.ForeColor = AppTheme.TieuDeMau;
            _tenDau.TextAlign = ContentAlignment.MiddleLeft;
            _tenDau.Cursor = Cursors.Hand;
            _tenDau.Bounds = new Rectangle(this.Px(68), 0, this.Px(228), this.Px(66));
            _tenDau.Click += (_, _) => ChonTab(0);

            _nav.CanGiuaTheoChu = true;
            _nav.VeDuongKeDay = false;
            _nav.DatChiSoKhongTruot(tabMacDau);
            _nav.Height = this.Px(67);
            _nav.DuocChon += ChonTab;

            _nutDangKy.Text = "Đăng ký";
            _nutDangKy.Kieu = KieuNut.Vien;
            _nutDangKy.BanKinhBo = 6;
            _nutDangKy.Font = AppTheme.ChuNut;
            _nutDangKy.Size = new Size(this.Px(104), this.Px(40));
            _nutDangKy.Click += (_, _) => _shell.HienDangKy();

            _nutDangNhap.Text = "Đăng nhập";
            _nutDangNhap.Kieu = KieuNut.Chinh;
            _nutDangNhap.BanKinhBo = 6;
            _nutDangNhap.Font = AppTheme.ChuNut;
            _nutDangNhap.Size = new Size(this.Px(118), this.Px(40));
            _nutDangNhap.Click += (_, _) => _shell.HienDangNhap();

            _dau.Controls.AddRange(new Control[] { _logoDau, _tenDau, _nav, _nutDangKy, _nutDangNhap });
            _dau.Resize += (_, _) => BoTriThanhDau();
            BoTriThanhDau();
        }

        private void BoTriThanhDau()
        {
            int pad = this.Px(28);
            int yNut = (_dau.Height - _nutDangNhap.Height) / 2;
            _nutDangNhap.Location = new Point(_dau.Width - pad - _nutDangNhap.Width, yNut);
            _nutDangKy.Location = new Point(_nutDangNhap.Left - this.Px(10) - _nutDangKy.Width, yNut);

            int gioiHanTrai = _tenDau.Right + this.Px(8);
            int gioiHanPhai = _nutDangKy.Left - this.Px(8);
            int wConLai = Math.Max(this.Px(220), gioiHanPhai - gioiHanTrai);
            int wNav = Math.Min(this.Px(410), wConLai);
            int xLyTuong = (_dau.Width - wNav) / 2;
            int xNav = Math.Clamp(xLyTuong, gioiHanTrai, Math.Max(gioiHanTrai, gioiHanPhai - wNav));
            _nav.SetBounds(xNav, 0, wNav, _dau.Height - 1);
        }

        private static void BatDoubleBuffer(Control c) =>
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(c, true);

        private void VeNenToanTrangChu(Graphics g, Control ctrl)
        {
            int wTong = Math.Max(1, _tabTrangChu.Width);
            int hTong = Math.Max(1, _tabTrangChu.Height);
            Point pt = _tabTrangChu.PointToClient(ctrl.PointToScreen(Point.Empty));

            GraphicsState st = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(-pt.X, -pt.Y);

            using (var bg = new LinearGradientBrush(
                       new Point(0, 0), new Point(wTong, hTong),
                       Color.FromArgb(249, 252, 255), Color.FromArgb(227, 238, 252)))
                g.FillRectangle(bg, 0, 0, wTong, hTong);

            using (var bWave1 = new SolidBrush(Color.FromArgb(54, 198, 222, 250)))
                g.FillEllipse(bWave1, wTong * 0.34f, -hTong * 0.12f, wTong * 0.88f, hTong * 1.25f);

            using (var bWave2 = new SolidBrush(Color.FromArgb(38, 180, 212, 248)))
                g.FillEllipse(bWave2, -wTong * 0.08f, hTong * 0.74f, wTong * 1.20f, hTong * 0.55f);

            g.Restore(st);
        }

        private void TaoTabTrangChu()
        {
            _tabTrangChu.Dock = DockStyle.Fill;
            _tabTrangChu.BackColor = AppTheme.NenGradTren;
            BatDoubleBuffer(_tabTrangChu);
            _tabTrangChu.Paint += (_, e) => VeNenToanTrangChu(e.Graphics, _tabTrangChu);

            _khungTrangChu.Size = new Size(this.Px(992), this.Px(484));
            _khungTrangChu.BackColor = AppTheme.NenGradTren;
            BatDoubleBuffer(_khungTrangChu);
            _khungTrangChu.Paint += (_, e) => VeNenToanTrangChu(e.Graphics, _khungTrangChu);

            _traiTrangChu.Bounds = new Rectangle(0, 0, this.Px(454), this.Px(484));
            _traiTrangChu.BackColor = AppTheme.NenGradTren;
            BatDoubleBuffer(_traiTrangChu);
            _traiTrangChu.Paint += (_, e) => VeNenToanTrangChu(e.Graphics, _traiTrangChu);

            var logoLon = new Logo { Bounds = new Rectangle(0, this.Px(10), this.Px(44), this.Px(44)) };

            var lblTieuDe = new TransparentGradientLabel
            {
                Font = AppTheme.TieuDeLon,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Hệ thống" + Environment.NewLine + "tra cứu sức khỏe",
                Bounds = new Rectangle(0, this.Px(62), this.Px(450), this.Px(96)),
                VeNenTheoCha = (g, c) => VeNenToanTrangChu(g, c)
            };

            _sloganBar = new SloganBar
            {
                Bounds = new Rectangle(0, this.Px(164), this.Px(454), this.Px(34)),
                VeNenTheoCha = (g, c) => VeNenToanTrangChu(g, c)
            };

            var lblMoTa = new TransparentGradientLabel
            {
                Font = AppTheme.ChuLon,
                ForeColor = AppTheme.ChuPhu,
                Text = "Tra cứu thông tin y tế nhanh chóng," + Environment.NewLine + "chính xác và đáng tin cậy.",
                Bounds = new Rectangle(0, this.Px(204), this.Px(446), this.Px(48)),
                VeNenTheoCha = (g, c) => VeNenToanTrangChu(g, c)
            };

            _nutBatDau.Text = "Bắt đầu tra cứu";
            _nutBatDau.Kieu = KieuNut.Chinh;
            _nutBatDau.BanKinhBo = 2;
            _nutBatDau.LucideIcon = LucideIconKind.Search;
            _nutBatDau.MuiTen = false;
            _nutBatDau.Font = AppTheme.ChuNut;
            _nutBatDau.Bounds = new Rectangle(0, this.Px(264), this.Px(216), this.Px(46));
            _nutBatDau.Click += (_, _) => YeuCauDangNhap();

            _traiTrangChu.Controls.AddRange(new Control[]
            {
                logoLon, lblTieuDe, _sloganBar, lblMoTa, _nutBatDau, TaoHangMuc(this.Px(332))
            });

            _minhHoa.Bounds = new Rectangle(this.Px(460), this.Px(4), this.Px(530), this.Px(476));
            _minhHoa.BackColor = AppTheme.NenGradTren;
            BatDoubleBuffer(_minhHoa);
            _minhHoa.Paint += (_, e) =>
            {
                VeNenToanTrangChu(e.Graphics, _minhHoa);
                Image? img = TaiNguyenAnh.Lay("hero_home.png");
                if (img != null)
                {
                    TaiNguyenAnh.VeAnhMemCanh(e.Graphics, "hero_home.png", img, _minhHoa.ClientRectangle, this.Px(36));
                }
                else
                {
                    LucideIcons.Draw(e.Graphics, LucideIconKind.HeartPulse,
                        new RectangleF(this.Px(140), this.Px(100), this.Px(240), this.Px(240)), AppTheme.XanhPhu, 1.9f);
                }
            };

            _khungTrangChu.Controls.Add(_traiTrangChu);
            _khungTrangChu.Controls.Add(_minhHoa);
            _tabTrangChu.Controls.Add(_khungTrangChu);
        }

        private Panel TaoHangMuc(int y)
        {
            int wTong = this.Px(450);
            int wCot = wTong / 3;
            string nl = Environment.NewLine;
            (LucideIconKind Ic, string Ten)[] muc =
            {
                (LucideIconKind.BookOpen, "Thông tin" + nl + "sức khỏe"),
                (LucideIconKind.Pill, "Tra cứu" + nl + "thuốc"),
                (LucideIconKind.HeartPulse, "Bảo vệ" + nl + "sức khỏe")
            };

            var hang = new Panel { Bounds = new Rectangle(0, y, wTong, this.Px(142)), BackColor = AppTheme.NenGradDuoi };
            BatDoubleBuffer(hang);
            hang.Paint += (_, e) =>
            {
                VeNenToanTrangChu(e.Graphics, hang);
                using var but = new Pen(Color.FromArgb(140, AppTheme.Vien));
                for (int i = 1; i < 3; i++)
                    e.Graphics.DrawLine(but, i * wCot, this.Px(10), i * wCot, this.Px(124));
            };

            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var tron = new Panel
                {
                    Bounds = new Rectangle(i * wCot + (wCot - this.Px(66)) / 2, this.Px(4), this.Px(66), this.Px(66)),
                    BackColor = AppTheme.NenGradDuoi,
                    Cursor = Cursors.Default
                };
                BatDoubleBuffer(tron);
                tron.Paint += (_, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    VeNenToanTrangChu(g, tron);

                    int d = this.Px(56);
                    var r = new RectangleF((tron.Width - d) / 2f, (tron.Height - d) / 2f, d, d);
                    float ht = _hoverMucT[idx];
                    Color vienMau = AppTheme.Tron(AppTheme.VienO, AppTheme.ChinhSang, ht);
                    float doDayVien = 1.4f + 0.8f * ht;

                    using (var b = new SolidBrush(AppTheme.NenTrang))
                        g.FillEllipse(b, r);
                    if (ht > 0.05f)
                    {
                        using var glow = new Pen(Color.FromArgb((int)(55 * ht), AppTheme.ChinhSang), 3.5f);
                        g.DrawEllipse(glow, r);
                    }
                    using (var p = new Pen(vienMau, doDayVien))
                        g.DrawEllipse(p, r);

                    int sz = this.Px(24);
                    LucideIcons.Draw(g, muc[idx].Ic,
                        new RectangleF(r.X + (d - sz) / 2f, r.Y + (d - sz) / 2f, sz, sz),
                        AppTheme.Tron(AppTheme.Chinh, AppTheme.ChinhSang, ht), 1.85f);
                };
                tron.MouseEnter += (_, _) => BatHoverMuc(idx, 1);
                tron.MouseLeave += (_, _) => BatHoverMuc(idx, 0);
                _tronMuc[idx] = tron;

                var lbl = new TransparentGradientLabel
                {
                    Font = AppTheme.ChuTheNho,
                    ForeColor = AppTheme.Chu,
                    Text = muc[i].Ten,
                    Centered = true,
                    Cursor = Cursors.Default,
                    Bounds = new Rectangle(i * wCot + this.Px(4), this.Px(74), wCot - this.Px(8), this.Px(54)),
                    VeNenTheoCha = (g, c) => VeNenToanTrangChu(g, c)
                };
                lbl.MouseEnter += (_, _) => BatHoverMuc(idx, 1);
                lbl.MouseLeave += (_, _) => BatHoverMuc(idx, 0);

                hang.Controls.Add(tron);
                hang.Controls.Add(lbl);
            }

            return hang;
        }

        private void BatHoverMuc(int idx, int dich)
        {
            _hoverMucDich[idx] = dich;
            if (!Visible || !IsHandleCreated || !SystemInformation.UIEffectsEnabled)
            {
                _hoverMucT[idx] = dich;
                _tronMuc[idx]?.Invalidate();
                return;
            }
            for (int i = 0; i < 3; i++) _hoverMucTu[i] = _hoverMucT[i];
            _swHoverMuc.Restart();
            _nhipHoverMuc.Start();
        }

        // Tab Gioi thieu Ban 9:
        //  - Nen xanh xam rat nhat (#F8FAFC) de cac the trang co vien ro + dai nhan Navy/Teal noi bat can bang (khong nhat nhoa, khong mau me)
        //  - Co dong noi dung suc tich, bo sung icon truc quan cho tung nhom
        //  - BO han khoi luu y canh bao trung lap (vi da co dong canh bao o day cua so)
        //  - Sua khuyet chu "quy" va khong de vien khung chan trang.
        private void TaoTabGioiThieu()
        {
            _tabGioiThieu.Dock = DockStyle.Fill;
            _tabGioiThieu.BackColor = AppTheme.Nen;

            int wKhung = this.Px(896);
            var khung = new Panel
            {
                Name = "khungGioiThieu",
                Size = new Size(wKhung, this.Px(900)),
                BackColor = AppTheme.Nen
            };

            int y = this.Px(14);
            int gianDong = this.Px(7);
            int gianDoan = this.Px(11);

            // 1. Phan mo dau ro net, co chu lon de doc cho nguoi lon tuoi
            khung.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuDocLoc,
                ForeColor = AppTheme.Chinh,
                Text = "GIỚI THIỆU ĐỀ TÀI",
                Bounds = new Rectangle(0, y, wKhung, this.Px(26))
            });
            y += this.Px(28);

            khung.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.TieuDeLon,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Hệ thống tra cứu triệu chứng và thông tin sức khỏe",
                Bounds = new Rectangle(0, y, wKhung, this.Px(48))
            });
            y += this.Px(54);

            const string chuMoDau = "Tập hợp thông tin triệu chứng, bệnh lý phổ biến và thuốc tham khảo trên một giao diện thống nhất — biên soạn từ các nguồn công khai uy tín (WHO, Bộ Y tế, MSD Manual).";
            var lblMoDau = new DoanChuThoang
            {
                Font = AppTheme.ChuDocMoDau,
                ForeColor = AppTheme.Chu,
                BackColor = AppTheme.Nen,
                Text = chuMoDau,
                GianDongPx = gianDong,
                GianDoanPx = gianDoan
            };
            int hMoDau = lblMoDau.TinhChieuCao(wKhung);
            lblMoDau.Bounds = new Rectangle(0, y, wKhung, hMoDau);
            khung.Controls.Add(lblMoDau);
            y += hMoDau + this.Px(22);

            // 2. Nhom 2 the lon co icon + dai nhan Navy / Teal, chu lon 11.5pt + gian dong thoang
            int gap2 = this.Px(20);
            int wCol2 = (wKhung - gap2) / 2;
            int pad2 = this.Px(22);
            int wChu2 = wCol2 - pad2 * 2;

            (LucideIconKind Ic, Color Mau, Color NenIc, string TieuDe, string NoiDung)[] haiCot =
            {
                (LucideIconKind.BookOpen, AppTheme.Chinh, AppTheme.ChipXanh, "Bối cảnh & nguồn biên soạn",
                 "• Khắc phục tình trạng thông tin sức khỏe rải rác, thiếu nguồn đối chiếu.\n" +
                 "• Dữ liệu phân danh mục tập trung, ghi rõ nguồn WHO, Bộ Y tế, MSD Manual.\n" +
                 "• Ứng dụng học tập phi thương mại, hoàn toàn không quảng cáo."),

                (LucideIconKind.Search, AppTheme.Phu, AppTheme.ChipMint, "Phạm vi tra cứu & mở rộng",
                 "• Chọn nhiều triệu chứng để gợi ý bệnh lý hoặc tìm trực tiếp tên bệnh, thuốc.\n" +
                 "• Xem mô tả ngắn, mức độ phổ biến, dấu hiệu cần đi khám và thuốc liên quan.\n" +
                 "• Lưu lịch sử theo tài khoản; định hướng mở rộng hồ sơ sức khỏe & nhắc tái khám.")
            };

            int maxNoiDung2 = 0;
            foreach (var c in haiCot)
                maxNoiDung2 = Math.Max(maxNoiDung2, ChuThoang.DoCao(c.NoiDung, AppTheme.ChuDoc, wChu2, gianDong, gianDoan));
            int hCol2 = this.Px(70) + maxNoiDung2 + this.Px(20);

            for (int i = 0; i < haiCot.Length; i++)
            {
                var c = haiCot[i];
                var box = TaoTheCoDai(new Rectangle(i * (wCol2 + gap2), y, wCol2, hCol2), c.Mau);
                int pad = pad2;
                int dIc = this.Px(38);
                box.Paint += (_, e) =>
                {
                    var rIc = new RectangleF(pad, this.Px(18), dIc, dIc);
                    using (var b = new SolidBrush(c.NenIc)) e.Graphics.FillEllipse(b, rIc);
                    using (var p = new Pen(Color.FromArgb(110, c.Mau), 1.2f)) e.Graphics.DrawEllipse(p, rIc);
                    int sz = this.Px(19);
                    LucideIcons.Draw(e.Graphics, c.Ic,
                        new RectangleF(rIc.X + (dIc - sz) / 2f, rIc.Y + (dIc - sz) / 2f, sz, sz), c.Mau, 1.9f);
                };
                box.Controls.Add(new Label
                {
                    AutoSize = false,
                    UseMnemonic = false,
                    Font = AppTheme.ChuDocMuc,
                    ForeColor = AppTheme.TieuDeMau,
                    Text = c.TieuDe,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Bounds = new Rectangle(pad + dIc + this.Px(12), this.Px(18), wCol2 - pad * 2 - dIc - this.Px(12), dIc)
                });
                box.Controls.Add(new DoanChuThoang
                {
                    Font = AppTheme.ChuDoc,
                    ForeColor = AppTheme.Chu,
                    BackColor = AppTheme.NenTrang,
                    Text = c.NoiDung,
                    GianDongPx = gianDong,
                    GianDoanPx = gianDoan,
                    Bounds = new Rectangle(pad, this.Px(68), wChu2, hCol2 - this.Px(78))
                });
                khung.Controls.Add(box);
            }
            y += hCol2 + this.Px(28);

            // 3. 4 nguyen tac hoat dong bo tri luoi 2x2 rong rai de chu 11.5pt thoang va cuc de doc
            khung.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuDocMucLon,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Nguyên tắc tổ chức thông tin",
                Bounds = new Rectangle(0, y, wKhung, this.Px(34))
            });
            y += this.Px(42);

            (LucideIconKind Ic, Color Mau, Color NenIc, string TieuDe, string MoTa)[] nguyenTac =
            {
                (LucideIconKind.Award, AppTheme.Chinh, AppTheme.ChipXanh, "Minh bạch nguồn", "Ghi rõ nguồn tham khảo công khai (WHO, Bộ Y tế, MSD Manual) ở từng mục dữ liệu."),
                (LucideIconKind.FileText, AppTheme.Phu, AppTheme.ChipMint, "Trình bày rõ ràng", "Nội dung ngắn gọn, dễ hiểu, có nêu rõ dấu hiệu cần đi khám sớm."),
                (LucideIconKind.Clock, AppTheme.Chinh, AppTheme.ChipXanh, "Lưu lịch sử tra cứu", "Tự động lưu lịch sử tra cứu riêng theo từng tài khoản để xem lại thuận tiện."),
                (LucideIconKind.Lock, AppTheme.Phu, AppTheme.ChipMint, "Phân quyền an toàn", "Tách bạch rõ quyền người dùng và quản trị viên nhằm bảo vệ dữ liệu hệ thống.")
            };

            int pad4 = this.Px(20);
            int dIc4 = this.Px(38);
            int wText4 = wCol2 - pad4 * 2 - dIc4 - this.Px(14);
            int maxMoTa4 = 0;
            foreach (var nt in nguyenTac)
                maxMoTa4 = Math.Max(maxMoTa4, ChuThoang.DoCao(nt.MoTa, AppTheme.ChuDoc, wText4, gianDong, gianDoan));
            int hRow4 = Math.Max(this.Px(112), this.Px(52) + maxMoTa4 + this.Px(18));

            for (int i = 0; i < nguyenTac.Length; i++)
            {
                int col = i % 2;
                int row = i / 2;
                var nt = nguyenTac[i];
                int cx = col * (wCol2 + gap2);
                int cy = y + row * (hRow4 + this.Px(16));

                var card = TaoTheToiGian(new Rectangle(cx, cy, wCol2, hRow4));
                card.Paint += (_, e) =>
                {
                    var rIc = new RectangleF(pad4, this.Px(20), dIc4, dIc4);
                    using (var b = new SolidBrush(nt.NenIc)) e.Graphics.FillEllipse(b, rIc);
                    int sz = this.Px(19);
                    LucideIcons.Draw(e.Graphics, nt.Ic,
                        new RectangleF(rIc.X + (dIc4 - sz) / 2f, rIc.Y + (dIc4 - sz) / 2f, sz, sz), nt.Mau, 1.9f);
                };
                int xTxt = pad4 + dIc4 + this.Px(14);
                card.Controls.Add(new Label
                {
                    AutoSize = false,
                    UseMnemonic = false,
                    Font = AppTheme.ChuDocMuc,
                    ForeColor = AppTheme.TieuDeMau,
                    Text = nt.TieuDe,
                    Bounds = new Rectangle(xTxt, this.Px(18), wText4, this.Px(28))
                });
                card.Controls.Add(new DoanChuThoang
                {
                    Font = AppTheme.ChuDoc,
                    ForeColor = AppTheme.Chu,
                    BackColor = AppTheme.NenTrang,
                    Text = nt.MoTa,
                    GianDongPx = gianDong,
                    GianDoanPx = gianDoan,
                    Bounds = new Rectangle(xTxt, this.Px(50), wText4, hRow4 - this.Px(60))
                });
                khung.Controls.Add(card);
            }
            y += hRow4 * 2 + this.Px(16) + this.Px(26);

            // 4. Chan trang gon, chu lon de nhin
            var chanStrip = TaoTheToiGian(new Rectangle(0, y, wKhung, this.Px(74)), AppTheme.NenTrang);
            chanStrip.Paint += (_, e) =>
            {
                int sz = this.Px(20);
                LucideIcons.Draw(e.Graphics, LucideIconKind.HeartPulse,
                    new RectangleF(this.Px(20), (chanStrip.Height - sz) / 2f, sz, sz), AppTheme.Chinh, 1.9f);
            };
            chanStrip.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.KhauHieuLon,
                ForeColor = AppTheme.Chinh,
                BackColor = AppTheme.NenTrang,
                Text = "Sức khỏe là tài sản quý giá nhất",
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(this.Px(48), this.Px(14), this.Px(370), this.Px(46))
            });

            var btnCs = new FlatButton
            {
                Kieu = KieuNut.Chu,
                Text = "Chính sách quyền riêng tư",
                Font = AppTheme.ChuDocDam,
                Bounds = new Rectangle(this.Px(440), this.Px(19), this.Px(232), this.Px(36))
            };
            btnCs.Click += (_, _) => { using var dlg = new HopThoaiChinhSach(); dlg.ShowDialog(FindForm()); };

            var btnStart = new FlatButton
            {
                Kieu = KieuNut.Chinh,
                BanKinhBo = 2,
                LucideIcon = LucideIconKind.Search,
                MuiTen = false,
                Text = "Bắt đầu tra cứu",
                Font = AppTheme.ChuDocDam,
                Bounds = new Rectangle(wKhung - this.Px(210), this.Px(14), this.Px(192), this.Px(46))
            };
            btnStart.Click += (_, _) => YeuCauDangNhap();
            chanStrip.Controls.AddRange(new Control[] { btnCs, btnStart });
            khung.Controls.Add(chanStrip);

            khung.Height = chanStrip.Bottom + this.Px(22);
            _tabGioiThieu.Controls.Add(khung);
        }

        private int DoCaoChu(string chu, Font font, int rong, int buPx = 14)
        {
            int wDo = Math.Max(40, rong - this.Px(4));
            Size sz = TextRenderer.MeasureText(chu, font, new Size(wDo, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            return sz.Height + this.Px(buPx);
        }

        private Panel TaoTheCoDai(Rectangle bounds, Color mauDai)
        {
            var p = new Panel { Bounds = bounds, BackColor = AppTheme.NenTrang };
            p.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, p.Width - 1.5f, p.Height - 1.5f);
                using var path = AppTheme.DuongBoGoc(r, this.Px(AppTheme.BoGocThe));
                using (var nen = new SolidBrush(AppTheme.NenTrang))
                    e.Graphics.FillPath(nen, path);
                var st = e.Graphics.Save();
                e.Graphics.SetClip(path, CombineMode.Intersect);
                using (var bDai = new SolidBrush(mauDai))
                    e.Graphics.FillRectangle(bDai, 0, 0, p.Width, this.Px(3));
                e.Graphics.Restore(st);
                using var but = new Pen(AppTheme.VienO, 1f);
                e.Graphics.DrawPath(but, path);
            };
            return p;
        }

        private Panel TaoTheToiGian(Rectangle bounds, Color? mauNen = null)
        {
            Color bgCol = mauNen ?? AppTheme.NenTrang;
            var p = new Panel { Bounds = bounds, BackColor = bgCol };
            p.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, p.Width - 1.5f, p.Height - 1.5f);
                using var path = AppTheme.DuongBoGoc(r, this.Px(AppTheme.BoGocThe));
                using var nen = new SolidBrush(bgCol);
                using var but = new Pen(AppTheme.VienO, 1f);
                e.Graphics.FillPath(nen, path);
                e.Graphics.DrawPath(but, path);
            };
            return p;
        }

        private void TaoTabHoTro()
        {
            _tabHoTro.Dock = DockStyle.Fill;
            _tabHoTro.BackColor = AppTheme.Nen;

            int wKhung = this.Px(896);
            int gianDong = this.Px(7);
            int gianDoan = this.Px(10);

            _khungHoTro = new Panel
            {
                Name = "khungHoTro",
                Size = new Size(wKhung, this.Px(1000)),
                BackColor = AppTheme.Nen
            };

            int y = this.Px(10);
            _khungHoTro.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.TieuDeLon,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Hỗ trợ - Câu hỏi thường gặp",
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(0, y, wKhung, this.Px(48))
            });
            y += this.Px(52);

            const string moTaHoTro = "Hướng dẫn nhanh 3 bước sử dụng và giải đáp các thắc mắc phổ biến khi thao tác.";
            var lblMoTaHt = new DoanChuThoang
            {
                Font = AppTheme.ChuDocMoDau,
                ForeColor = AppTheme.Chu,
                BackColor = AppTheme.Nen,
                Text = moTaHoTro,
                GianDongPx = gianDong,
                GianDoanPx = gianDoan
            };
            int hMoTaHt = lblMoTaHt.TinhChieuCao(wKhung);
            lblMoTaHt.Bounds = new Rectangle(0, y, wKhung, hMoTaHt);
            _khungHoTro.Controls.Add(lblMoTaHt);
            y += hMoTaHt + this.Px(18);

            (string Buoc, string TieuDe, string NoiDung)[] buoc =
            {
                ("01", "Đăng ký hoặc Đăng nhập",
                 "Tạo tài khoản bằng họ tên, tên đăng nhập và email hợp lệ, hoặc đăng nhập nếu đã có tài khoản."),
                ("02", "Nhập triệu chứng hoặc tên thuốc",
                 "Gõ từ khóa hoặc chọn nhóm triệu chứng để hệ thống tìm kiếm và trả về danh sách phù hợp."),
                ("03", "Đọc chi tiết và gửi góp ý",
                 "Xem mô tả bệnh lý, thuốc liên quan, lưu lịch sử hoặc gửi góp ý trực tiếp cho quản trị viên.")
            };

            int gap = this.Px(18);
            int wThe = (wKhung - gap * 2) / 3;
            int padBuoc = this.Px(20);
            int wChuBuoc = wThe - padBuoc * 2;
            int maxBuocTd = 0;
            int maxBuocNd = 0;
            foreach (var b in buoc)
            {
                maxBuocTd = Math.Max(maxBuocTd, ChuThoang.DoCao(b.TieuDe, AppTheme.ChuDocMuc, wChuBuoc, this.Px(4), 0));
                maxBuocNd = Math.Max(maxBuocNd, ChuThoang.DoCao(b.NoiDung, AppTheme.ChuDoc, wChuBuoc, gianDong, gianDoan));
            }
            int hBuoc = this.Px(48) + maxBuocTd + this.Px(10) + maxBuocNd + this.Px(18);
            for (int i = 0; i < buoc.Length; i++)
            {
                var the = TaoTheBuocToiGian(buoc[i].Buoc, buoc[i].TieuDe, buoc[i].NoiDung,
                    new Rectangle(i * (wThe + gap), y, wThe, hBuoc), maxBuocTd, gianDong, gianDoan);
                _khungHoTro.Controls.Add(the);
            }

            int yFaqHeader = y + hBuoc + this.Px(28);
            _khungHoTro.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuDocMucLon,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Câu hỏi thường gặp (FAQ)",
                TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(0, yFaqHeader, this.Px(420), this.Px(44))
            });

            _oTimFaq = new FormField
            {
                GonGang = true,
                IconTrai = LucideIconKind.Search,
                HienNutXoa = true,
                PlaceholderText = "Gõ từ khóa để lọc câu hỏi...",
                Bounds = new Rectangle(wKhung - this.Px(350), yFaqHeader - this.Px(1), this.Px(350), this.Px(46))
            };
            _oTimFaq.TextChanged += (_, _) => _faq?.Loc(_oTimFaq.Text);
            _khungHoTro.Controls.Add(_oTimFaq);

            const string NhomTaiKhoan = "Tài khoản";
            const string NhomTraCuu = "Tra cứu";
            const string NhomBaoMat = "Bảo mật";
            const string NhomLienHe = "Liên hệ - Góp ý";

            (string Nhom, string Hoi, string Dap)[] dsFaq =
            {
                (NhomTaiKhoan, "Tôi có bắt buộc phải tạo tài khoản trước khi sử dụng không?",
                 "Có. Lịch sử tra cứu và hồ sơ sức khỏe được lưu riêng theo từng tài khoản cá nhân nên bạn cần đăng ký và đăng nhập để sử dụng đầy đủ chức năng."),
                (NhomTaiKhoan, "Nếu quên mật khẩu thì tôi khôi phục bằng cách nào?",
                 "Tại màn hình Đăng nhập, nhấn vào \"Quên mật khẩu?\". Hệ thống sẽ mở hộp thoại khôi phục qua email đã đăng ký (gửi mã xác nhận 6 số hiệu lực 60 giây → xác minh mã → đặt mật khẩu mới)."),
                (NhomTaiKhoan, "Tôi có thể yêu cầu xóa tài khoản hoặc xóa lịch sử tra cứu không?",
                 "Có. Bạn có thể xóa lịch sử tra cứu trong trang tài khoản, hoặc gửi yêu cầu xóa dữ liệu tài khoản qua mục Góp ý - Liên hệ quản trị viên sau khi đăng nhập."),
                (NhomTaiKhoan, "Tại sao khi đăng nhập lại báo \"Không kết nối được tới máy chủ\"?",
                 "Lỗi này xảy ra khi hệ thống máy chủ chưa khởi động hoặc kết nối mạng cục bộ đang gián đoạn. Hãy kiểm tra lại kết nối và thử đăng nhập lại."),

                (NhomTraCuu, "Làm thế nào để bắt đầu tra cứu triệu chứng hoặc tên thuốc?",
                 "Bạn nhấn nút \"Bắt đầu tra cứu\" hoặc \"Đăng nhập\". Sau khi vào trang chính, bạn có thể nhập từ khóa triệu chứng, bệnh lý hoặc tên thuốc để nhận kết quả phù hợp."),
                (NhomTraCuu, "Tôi có thể chọn nhiều triệu chứng cùng lúc khi tra cứu không?",
                 "Có. Bạn có thể chọn nhiều triệu chứng thuộc các nhóm cơ quan (như Hô hấp, Tiêu hóa, Da liễu, Toàn thân...) hoặc tìm trực tiếp theo tên bệnh/thuốc."),
                (NhomTraCuu, "Dữ liệu bệnh lý và thuốc trong hệ thống lấy từ nguồn nào?",
                 "Nội dung tham khảo được tổng hợp từ các tài liệu y khoa công khai uy tín như WHO, Bộ Y tế Việt Nam và MSD Manual nhằm bảo đảm thông tin rõ ràng, chọn lọc."),
                (NhomTraCuu, "Ứng dụng có sử dụng được khi không có kết nối mạng không?",
                 "Không. Ứng dụng cần kết nối tới hệ thống dữ liệu để xác thực tài khoản, đồng bộ lịch sử và tra cứu danh mục bệnh lý, thuốc mới nhất."),
                (NhomTraCuu, "Tôi có thể dùng tài khoản của mình để tra cứu giúp người thân không?",
                 "Có. Bạn hoàn toàn có thể tra cứu thông tin tham khảo cho người thân. Tuy nhiên, nếu xuất hiện dấu hiệu cảnh báo nghiêm trọng, hãy đưa người bệnh đến cơ sở y tế gần nhất."),

                (NhomBaoMat, "Mật khẩu và thông tin đăng nhập được bảo vệ như thế nào?",
                 "Mật khẩu được kiểm tra độ mạnh (8–64 ký tự, có chữ và số) và được bảo vệ an toàn trước khi lưu trữ. Bạn cũng có thể bật hoặc tắt tùy chọn \"Nhớ mật khẩu\" bất kỳ lúc nào."),
                (NhomBaoMat, "Lịch sử tra cứu của tôi có bị chia sẻ cho bên thứ ba không?",
                 "Không. Đây là đồ án học tập của nhóm sinh viên, dữ liệu chỉ phục vụ quá trình sử dụng ứng dụng và không bán hay chia sẻ cho bên thứ ba."),
                (NhomBaoMat, "Thông tin tra cứu trên phần mềm có thay thế kết luận của bác sĩ không?",
                 "Không. Mọi thông tin triệu chứng, bệnh lý và thuốc chỉ mang tính chất tham khảo dựa trên tài liệu y khoa. Nếu tình trạng sức khỏe nghiêm trọng hoặc kéo dài, bạn cần đi khám trực tiếp."),

                (NhomLienHe, "Làm thế nào để gửi góp ý, báo lỗi hoặc khiếu nại tới quản trị viên?",
                 "Sau khi đăng nhập vào ứng dụng, bạn mở mục \"Góp ý - Liên hệ Admin\". Tại đây bạn có thể chọn loại phản ánh (góp ý, báo lỗi, khiếu nại), nhập nội dung chi tiết và đính kèm tệp hình ảnh hoặc tài liệu minh chứng gửi trực tiếp tới quản trị viên."),
                (NhomLienHe, "Tôi phát hiện thông tin bệnh lý hoặc thuốc chưa chính xác thì báo ở đâu?",
                 "Bạn vui lòng vào mục \"Góp ý - Liên hệ Admin\" sau khi đăng nhập, ghi rõ tên bệnh lý hoặc tên thuốc cần kiểm tra và đính kèm tệp tài liệu tham khảo (nếu có) để quản trị viên rà soát, cập nhật."),
                (NhomLienHe, "Yêu cầu góp ý hoặc khiếu nại được tiếp nhận và xử lý như thế nào?",
                 "Mọi phản ánh gửi kèm tệp từ người dùng sẽ được chuyển tới hàng đợi quản trị viên để kiểm tra và phản hồi. Do đây là sản phẩm đồ án sinh viên, nhóm sẽ rà soát định kỳ trong quá trình vận hành môn học.")
            };

            var thanhNhom = new FlowLayoutPanel
            {
                Location = new Point(0, yFaqHeader + this.Px(58)),
                Width = wKhung,
                Height = this.Px(44),
                BackColor = AppTheme.Nen,
                WrapContents = true
            };

            (string Nhom, string Nhan)[] nhomTab =
            {
                (string.Empty, $"Tất cả ({dsFaq.Length})"),
                (NhomTaiKhoan, $"{NhomTaiKhoan} ({dsFaq.Count(x => x.Nhom == NhomTaiKhoan)})"),
                (NhomTraCuu, $"{NhomTraCuu} ({dsFaq.Count(x => x.Nhom == NhomTraCuu)})"),
                (NhomBaoMat, $"{NhomBaoMat} ({dsFaq.Count(x => x.Nhom == NhomBaoMat)})"),
                (NhomLienHe, $"{NhomLienHe} ({dsFaq.Count(x => x.Nhom == NhomLienHe)})")
            };

            _nutNhomFaq.Clear();
            int tongRongNut = 0;
            foreach (var (nhom, nhan) in nhomTab)
            {
                Size sz = TextRenderer.MeasureText(nhan, AppTheme.ChuDocLoc, Size.Empty, TextFormatFlags.NoPrefix);
                int wBtn = sz.Width + this.Px(26);
                var btnNhom = new FlatButton
                {
                    Text = nhan,
                    Font = AppTheme.ChuDocLoc,
                    BanKinhBo = 4,
                    Kieu = nhom.Length == 0 ? KieuNut.Chinh : KieuNut.Vien,
                    Size = new Size(wBtn, this.Px(38)),
                    Margin = new Padding(0, 0, this.Px(10), this.Px(6))
                };
                tongRongNut += wBtn + this.Px(10);
                string nhomCapture = nhom;
                btnNhom.Click += (_, _) => ChonNhomFaq(nhomCapture);
                _nutNhomFaq.Add((btnNhom, nhomCapture));
                thanhNhom.Controls.Add(btnNhom);
            }
            if (tongRongNut > wKhung) thanhNhom.Height = this.Px(88);
            _khungHoTro.Controls.Add(thanhNhom);

            _faq = new FaqAccordion(dsFaq)
            {
                Location = new Point(0, thanhNhom.Bottom + this.Px(12)),
                Width = wKhung
            };
            _faq.Height = _faq.ChieuCaoCanThiet();
            _faq.ChieuCaoDoi += (_, _) => CapNhatBoTriHoTro();
            _khungHoTro.Controls.Add(_faq);

            const string ndHopHoTro =
                "• Góp ý - khiếu nại: Đăng nhập và mở mục \"Góp ý - Liên hệ Admin\" (hỗ trợ đính kèm tệp).\n" +
                "• Quên mật khẩu: Chọn \"Quên mật khẩu?\" tại màn hình Đăng nhập để khôi phục qua email.";
            int wHopChu = wKhung - this.Px(248);
            int hHopNd = ChuThoang.DoCao(ndHopHoTro, AppTheme.ChuDoc, wHopChu, gianDong, gianDoan);
            int hHopTong = Math.Max(this.Px(118), this.Px(54) + hHopNd + this.Px(18));
            _hopHoTro = TaoTheToiGian(new Rectangle(0, _faq.Bottom + this.Px(22), wKhung, hHopTong));
            _hopHoTro.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuDocMuc,
                ForeColor = AppTheme.TieuDeMau,
                Text = "Cần gửi góp ý, khiếu nại hoặc hỗ trợ thêm?",
                Bounds = new Rectangle(this.Px(22), this.Px(16), wHopChu, this.Px(30))
            });
            _hopHoTro.Controls.Add(new DoanChuThoang
            {
                Font = AppTheme.ChuDoc,
                ForeColor = AppTheme.Chu,
                BackColor = AppTheme.NenTrang,
                Text = ndHopHoTro,
                GianDongPx = gianDong,
                GianDoanPx = gianDoan,
                Bounds = new Rectangle(this.Px(22), this.Px(52), wHopChu, hHopNd)
            });
            var nut = new FlatButton
            {
                Kieu = KieuNut.Chinh,
                BanKinhBo = 2,
                LucideIcon = LucideIconKind.Search,
                MuiTen = false,
                Text = "Bắt đầu tra cứu",
                Font = AppTheme.ChuDocDam,
                Bounds = new Rectangle(wKhung - this.Px(212), (hHopTong - this.Px(46)) / 2, this.Px(192), this.Px(46))
            };
            nut.Click += (_, _) => YeuCauDangNhap();
            _hopHoTro.Controls.Add(nut);
            _khungHoTro.Controls.Add(_hopHoTro);

            CapNhatBoTriHoTro();
            _tabHoTro.Controls.Add(_khungHoTro);
        }

        private void ChonNhomFaq(string nhom)
        {
            _nhomFaqDangChon = nhom;
            foreach (var (btn, tenNhom) in _nutNhomFaq)
            {
                btn.Kieu = tenNhom.Equals(_nhomFaqDangChon, StringComparison.OrdinalIgnoreCase)
                    ? KieuNut.Chinh
                    : KieuNut.Vien;
                btn.Invalidate();
            }
            _faq?.LocTheoNhom(_nhomFaqDangChon);
        }

        private void CapNhatBoTriHoTro()
        {
            if (_khungHoTro == null || _faq == null || _hopHoTro == null) return;
            _khungHoTro.SuspendLayout();
            _hopHoTro.Top = _faq.Bottom + this.Px(22);
            _khungHoTro.Height = _hopHoTro.Bottom + this.Px(22);
            _khungHoTro.ResumeLayout(false);
            CanGiuaNoiDung();
        }

        private Panel TaoTheBuocToiGian(string so, string tieuDe, string noiDung, Rectangle bounds, int hTieuDe, int gianDong, int gianDoan)
        {
            var the = TaoTheCoDai(bounds, AppTheme.Chinh);
            int pad = this.Px(20);
            int w = bounds.Width - pad * 2;

            the.Controls.Add(new Label
            {
                AutoSize = false,
                UseMnemonic = false,
                Font = AppTheme.ChuDocDam,
                ForeColor = AppTheme.Chinh,
                Text = "Bước " + so,
                Bounds = new Rectangle(pad, this.Px(16), w, this.Px(26))
            });
            the.Controls.Add(new DoanChuThoang
            {
                Font = AppTheme.ChuDocMuc,
                ForeColor = AppTheme.TieuDeMau,
                BackColor = AppTheme.NenTrang,
                Text = tieuDe,
                GianDongPx = this.Px(4),
                GianDoanPx = 0,
                Bounds = new Rectangle(pad, this.Px(46), w, hTieuDe)
            });
            int yNd = this.Px(46) + hTieuDe + this.Px(8);
            the.Controls.Add(new DoanChuThoang
            {
                Font = AppTheme.ChuDoc,
                ForeColor = AppTheme.Chu,
                BackColor = AppTheme.NenTrang,
                Text = noiDung,
                GianDongPx = gianDong,
                GianDoanPx = gianDoan,
                Bounds = new Rectangle(pad, yNd, w, bounds.Height - yNd - this.Px(12))
            });
            return the;
        }

        public void ChonTab(int chiSo)
        {
            chiSo = Math.Clamp(chiSo, 0, 2);
            _nav.ChiSo = chiSo;

            void DoiTab()
            {
                _tabTrangChu.Visible = chiSo == 0;
                _tabGioiThieu.Visible = chiSo == 1;
                _tabHoTro.Visible = chiSo == 2;
                _than.BackColor = AppTheme.Nen;
                _than.AutoScrollPosition = Point.Empty;
                CanGiuaNoiDung();
            }

            if (_tabHienTai >= 0 && chiSo != _tabHienTai && IsHandleCreated && Visible)
            {
                int huong = chiSo > _tabHienTai ? 1 : -1;
                _tabHienTai = chiSo;
                HieuUngChuyen.Chay(_than, DoiTab, huong);
            }
            else
            {
                _tabHienTai = chiSo;
                DoiTab();
            }

            if (FindForm() is Form f)
                f.AcceptButton = NutMacDinh;
        }

        private void YeuCauDangNhap() =>
            _shell.HienDangNhap("Vui lòng đăng nhập để bắt đầu tra cứu triệu chứng và thuốc.");

        private void CanGiuaNoiDung()
        {
            Control? khung = _nav.ChiSo switch
            {
                0 => _khungTrangChu,
                1 => _tabGioiThieu.Controls.Count > 0 ? _tabGioiThieu.Controls[0] : null,
                _ => _khungHoTro
            };
            if (khung == null) return;

            int leDoc = this.Px(14);
            int tongCao = khung.Height + leDoc * 2;
            bool canCuon = tongCao > _than.ClientSize.Height + 2;
            _than.AutoScrollMinSize = canCuon ? new Size(0, tongCao) : Size.Empty;

            khung.Left = Math.Max(this.Px(14), (_than.ClientSize.Width - khung.Width) / 2);
            int yLyTuong = Math.Max(leDoc, (_than.ClientSize.Height - khung.Height) / 2);
            khung.Top = canCuon ? Math.Min(leDoc, yLyTuong) : yLyTuong;
        }

        private sealed class TransparentGradientLabel : Control
        {
            public Action<Graphics, Control>? VeNenTheoCha;
            public bool Centered;

            public TransparentGradientLabel()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                TabStop = false;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                VeNenTheoCha?.Invoke(e.Graphics, this);
                TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix |
                    (Centered ? TextFormatFlags.HorizontalCenter | TextFormatFlags.Top : TextFormatFlags.Left | TextFormatFlags.Top);
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, flags);
            }
        }

        private sealed class SloganBar : Control
        {
            private const int StaggerMs = 70;
            private const int ItemDurMs = 200;
            private const int TongMs = StaggerMs * 2 + ItemDurMs;

            private static readonly (LucideIconKind Ic, string Chu)[] Items =
            {
                (LucideIconKind.Award, "Tin cậy"),
                (LucideIconKind.Heart, "Gần gũi"),
                (LucideIconKind.Users, "Đồng hành")
            };

            public Action<Graphics, Control>? VeNenTheoCha;

            private readonly Stopwatch _sw = new();
            private readonly System.Windows.Forms.Timer _nhip = new() { Interval = 16 };
            private bool _daChayVao;
            private bool _dangChay;

            public SloganBar()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                TabStop = false;
                _nhip.Tick += (_, _) =>
                {
                    if (!Visible || _sw.ElapsedMilliseconds >= TongMs)
                    {
                        _dangChay = false;
                        _nhip.Stop();
                    }
                    Invalidate();
                };
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                if (!_daChayVao && !DesignMode && SystemInformation.UIEffectsEnabled)
                {
                    _daChayVao = true;
                    _dangChay = true;
                    _sw.Restart();
                    _nhip.Start();
                }
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                if (!Visible)
                {
                    _dangChay = false;
                    _nhip.Stop();
                }
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

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                VeNenTheoCha?.Invoke(g, this);

                long elapsed = _dangChay ? _sw.ElapsedMilliseconds : TongMs;
                int x = 0;
                int szIc = this.Px(16);
                int gapIc = this.Px(6);
                int gapSep = this.Px(14);

                for (int i = 0; i < Items.Length; i++)
                {
                    var it = Items[i];
                    float localT = Math.Clamp((elapsed - i * StaggerMs) / (float)ItemDurMs, 0f, 1f);
                    float k = AppTheme.EaseOutCubic(localT);
                    int alpha = (int)MathF.Round(255f * k);
                    float dx = this.Px(8) * (1f - k);

                    int wChu = TextRenderer.MeasureText(g, it.Chu, AppTheme.KhauHieu, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;

                    if (alpha > 4)
                    {
                        LucideIcons.Draw(g, it.Ic,
                            new RectangleF(x + dx, (Height - szIc) / 2f, szIc, szIc),
                            Color.FromArgb(alpha, AppTheme.Chinh), 1.85f);

                        TextRenderer.DrawText(g, it.Chu, AppTheme.KhauHieu,
                            new Rectangle((int)(x + dx + szIc + gapIc), 0, wChu + this.Px(4), Height),
                            Color.FromArgb(alpha, AppTheme.TieuDeMau),
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                    }

                    x += szIc + gapIc + wChu;

                    if (i < Items.Length - 1)
                    {
                        x += gapSep;
                        if (alpha > 8)
                        {
                            using var pSep = new Pen(Color.FromArgb((int)(alpha * 0.7f), AppTheme.VienDam), 1.5f);
                            g.DrawLine(pSep, x, this.Px(8), x, Height - this.Px(8));
                        }
                        x += gapSep;
                    }
                }
            }
        }
    }
}
