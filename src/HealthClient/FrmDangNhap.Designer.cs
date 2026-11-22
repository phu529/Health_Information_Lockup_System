#nullable disable
using System.Drawing;
using System.Windows.Forms;

namespace HealthClient
{
    partial class FrmDangNhap
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // v17: chia vung bang Dock (khong con chan trang cao co dinh dat toa do tuyet doi):
        //   thanhTab        Dock = Top      (Dang nhap | Dang ky, gach chan tu ve)
        //   canhBaoYTe      Dock = Bottom   (tu do chieu cao theo chu, luon hien du)
        //   pnlHanhDong     Dock = Bottom   (nut Dang nhap, dang nhap nhanh, Dang ky tai khoan)
        //   pnlCuon         Dock = Fill     (huy hieu, tieu de, o nhap; cuon duoc neu man hinh thap)
        // Thu tu Controls.Add o cuoi ham quyet dinh thu tu chia vung: Fill truoc, vung sat dinh/day sau.
        private void InitializeComponent()
        {
            this.pnlThuongHieu = new HealthClient.UI.PanelThuongHieu();
            this.pnlNoiDung = new System.Windows.Forms.Panel();
            this.pnlCuon = new System.Windows.Forms.Panel();
            this.pnlHanhDong = new System.Windows.Forms.Panel();
            this.thanhTab = new HealthClient.UI.ThanhTab();
            this.canhBaoYTe = new HealthClient.UI.CanhBaoYTe();
            this.huyHieu = new HealthClient.UI.HuyHieu();
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblTenDangNhap = new System.Windows.Forms.Label();
            this.txtTenDangNhap = new HealthClient.UI.OnhapLieu();
            this.lblLoiTenDangNhap = new System.Windows.Forms.Label();
            this.lblMatKhau = new System.Windows.Forms.Label();
            this.txtMatKhau = new HealthClient.UI.OnhapLieu();
            this.lblLoiMatKhau = new System.Windows.Forms.Label();
            this.chkNhoTaiKhoan = new HealthClient.UI.HopChon();
            this.lnkQuenMatKhau = new System.Windows.Forms.LinkLabel();
            this.lblLoiChung = new System.Windows.Forms.Label();
            this.btnDangNhap = new HealthClient.UI.NutBoGoc();
            this.pnlHoacTrai = new System.Windows.Forms.Panel();
            this.lblHoac = new System.Windows.Forms.Label();
            this.pnlHoacPhai = new System.Windows.Forms.Panel();
            this.btnVssId = new HealthClient.UI.NutBoGoc();
            this.btnGoogle = new HealthClient.UI.NutBoGoc();
            this.lnkChuyenDangKy = new System.Windows.Forms.LinkLabel();
            this.pnlNoiDung.SuspendLayout();
            this.pnlCuon.SuspendLayout();
            this.pnlHanhDong.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlThuongHieu
            //
            this.pnlThuongHieu.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlThuongHieu.Location = new System.Drawing.Point(0, 0);
            this.pnlThuongHieu.Name = "pnlThuongHieu";
            this.pnlThuongHieu.Size = new System.Drawing.Size(360, 660);
            this.pnlThuongHieu.TabIndex = 1;
            this.pnlThuongHieu.TabStop = false;
            //
            // pnlNoiDung
            //
            this.pnlNoiDung.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.pnlNoiDung.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNoiDung.Location = new System.Drawing.Point(360, 0);
            this.pnlNoiDung.Name = "pnlNoiDung";
            this.pnlNoiDung.Size = new System.Drawing.Size(560, 660);
            this.pnlNoiDung.TabIndex = 0;
            //
            // thanhTab
            //
            this.thanhTab.Dock = System.Windows.Forms.DockStyle.Top;
            this.thanhTab.Location = new System.Drawing.Point(0, 0);
            this.thanhTab.Name = "thanhTab";
            this.thanhTab.Size = new System.Drawing.Size(560, 60);
            this.thanhTab.TabDangChon = 0;
            this.thanhTab.ChiHienTab = 0;
            this.thanhTab.TabIndex = 20;
            this.thanhTab.TabBam += new System.EventHandler<int>(this.thanhTab_TabBam);
            //
            // canhBaoYTe (Dock Bottom, tu tinh chieu cao theo chu)
            //
            this.canhBaoYTe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.canhBaoYTe.Location = new System.Drawing.Point(0, 580);
            this.canhBaoYTe.Name = "canhBaoYTe";
            this.canhBaoYTe.Size = new System.Drawing.Size(560, 80);
            this.canhBaoYTe.TabIndex = 21;
            //
            // pnlHanhDong
            //
            this.pnlHanhDong.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.pnlHanhDong.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHanhDong.Location = new System.Drawing.Point(0, 372);
            this.pnlHanhDong.Name = "pnlHanhDong";
            this.pnlHanhDong.Size = new System.Drawing.Size(560, 208);
            this.pnlHanhDong.TabIndex = 1;
            //
            // pnlCuon
            //
            this.pnlCuon.AutoScroll = true;
            this.pnlCuon.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.pnlCuon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCuon.Location = new System.Drawing.Point(0, 60);
            this.pnlCuon.Name = "pnlCuon";
            this.pnlCuon.Size = new System.Drawing.Size(560, 312);
            this.pnlCuon.TabIndex = 0;
            //
            // huyHieu
            //
            this.huyHieu.Location = new System.Drawing.Point(56, 10);
            this.huyHieu.MauCham = System.Drawing.Color.FromArgb(23, 71, 196);
            this.huyHieu.Name = "huyHieu";
            this.huyHieu.Size = new System.Drawing.Size(300, 28);
            this.huyHieu.TabIndex = 22;
            this.huyHieu.Text = "Chào mừng bạn đến với";
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = false;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(11, 31, 51);
            this.lblTieuDe.Location = new System.Drawing.Point(56, 42);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(448, 40);
            this.lblTieuDe.TabIndex = 23;
            this.lblTieuDe.Text = "Hệ thống tra cứu sức khỏe";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblTenDangNhap
            //
            this.lblTenDangNhap.AutoSize = false;
            this.lblTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTenDangNhap.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblTenDangNhap.Location = new System.Drawing.Point(56, 90);
            this.lblTenDangNhap.Name = "lblTenDangNhap";
            this.lblTenDangNhap.Size = new System.Drawing.Size(448, 22);
            this.lblTenDangNhap.TabIndex = 24;
            this.lblTenDangNhap.Text = "Tên đăng nhập";
            this.lblTenDangNhap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtTenDangNhap
            //
            this.txtTenDangNhap.BieuTuong = HealthClient.UI.BieuTuongO.NguoiDung;
            this.txtTenDangNhap.CoNutHienAn = false;
            this.txtTenDangNhap.CoNutXoa = true;
            this.txtTenDangNhap.GhiChu = "Gợi ý tên đăng nhập";
            this.txtTenDangNhap.NutGhiChuBam += new System.EventHandler(this.txtTenDangNhap_NutGhiChuBam);
            this.txtTenDangNhap.Location = new System.Drawing.Point(56, 112);
            this.txtTenDangNhap.MaxLength = 20;
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.PlaceholderText = "Nhập tên đăng nhập";
            this.txtTenDangNhap.Size = new System.Drawing.Size(448, 46);
            this.txtTenDangNhap.TabIndex = 0;
            //
            // lblLoiTenDangNhap
            //
            this.lblLoiTenDangNhap.AutoSize = false;
            this.lblLoiTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiTenDangNhap.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiTenDangNhap.Location = new System.Drawing.Point(56, 160);
            this.lblLoiTenDangNhap.Name = "lblLoiTenDangNhap";
            this.lblLoiTenDangNhap.Size = new System.Drawing.Size(448, 18);
            this.lblLoiTenDangNhap.TabIndex = 25;
            this.lblLoiTenDangNhap.Text = "";
            this.lblLoiTenDangNhap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblMatKhau
            //
            this.lblMatKhau.AutoSize = false;
            this.lblMatKhau.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblMatKhau.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblMatKhau.Location = new System.Drawing.Point(56, 182);
            this.lblMatKhau.Name = "lblMatKhau";
            this.lblMatKhau.Size = new System.Drawing.Size(448, 22);
            this.lblMatKhau.TabIndex = 26;
            this.lblMatKhau.Text = "Mật khẩu";
            this.lblMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtMatKhau
            //
            this.txtMatKhau.BieuTuong = HealthClient.UI.BieuTuongO.KhoaMatKhau;
            this.txtMatKhau.CoNutHienAn = true;
            this.txtMatKhau.Location = new System.Drawing.Point(56, 204);
            this.txtMatKhau.MaxLength = 64;
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.PlaceholderText = "Nhập mật khẩu";
            this.txtMatKhau.Size = new System.Drawing.Size(448, 46);
            this.txtMatKhau.TabIndex = 1;
            this.txtMatKhau.UseSystemPasswordChar = true;
            //
            // lblLoiMatKhau
            //
            this.lblLoiMatKhau.AutoSize = false;
            this.lblLoiMatKhau.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiMatKhau.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiMatKhau.Location = new System.Drawing.Point(56, 252);
            this.lblLoiMatKhau.Name = "lblLoiMatKhau";
            this.lblLoiMatKhau.Size = new System.Drawing.Size(448, 18);
            this.lblLoiMatKhau.TabIndex = 27;
            this.lblLoiMatKhau.Text = "";
            this.lblLoiMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // chkNhoTaiKhoan
            //
            this.chkNhoTaiKhoan.AutoSize = false;
            this.chkNhoTaiKhoan.Location = new System.Drawing.Point(56, 276);
            this.chkNhoTaiKhoan.Name = "chkNhoTaiKhoan";
            this.chkNhoTaiKhoan.Size = new System.Drawing.Size(240, 30);
            this.chkNhoTaiKhoan.TabIndex = 2;
            this.chkNhoTaiKhoan.Text = "Nhớ mật khẩu";
            this.chkNhoTaiKhoan.UseVisualStyleBackColor = false;
            //
            // lnkQuenMatKhau
            //
            this.lnkQuenMatKhau.ActiveLinkColor = System.Drawing.Color.FromArgb(12, 38, 128);
            this.lnkQuenMatKhau.AutoSize = false;
            this.lnkQuenMatKhau.Font = new System.Drawing.Font("Segoe UI", 11.5F);
            this.lnkQuenMatKhau.LinkArea = new System.Windows.Forms.LinkArea(0, 14);
            this.lnkQuenMatKhau.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.lnkQuenMatKhau.LinkColor = System.Drawing.Color.FromArgb(23, 71, 196);
            this.lnkQuenMatKhau.Location = new System.Drawing.Point(316, 276);
            this.lnkQuenMatKhau.Name = "lnkQuenMatKhau";
            this.lnkQuenMatKhau.Size = new System.Drawing.Size(188, 30);
            this.lnkQuenMatKhau.TabIndex = 3;
            this.lnkQuenMatKhau.TabStop = true;
            this.lnkQuenMatKhau.Text = "Quên mật khẩu?";
            this.lnkQuenMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkQuenMatKhau.VisitedLinkColor = System.Drawing.Color.FromArgb(23, 71, 196);
            this.lnkQuenMatKhau.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkQuenMatKhau_LinkClicked);
            //
            // lblLoiChung (nam tren nut Dang nhap)
            //
            this.lblLoiChung.AutoSize = false;
            this.lblLoiChung.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiChung.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiChung.Location = new System.Drawing.Point(56, 0);
            this.lblLoiChung.Name = "lblLoiChung";
            this.lblLoiChung.Size = new System.Drawing.Size(448, 20);
            this.lblLoiChung.TabIndex = 28;
            this.lblLoiChung.Text = "";
            this.lblLoiChung.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnDangNhap (nut chinh: navy, re chuot doi sang xanh)
            //
            this.btnDangNhap.BanKinh = 8;
            this.btnDangNhap.KieuVien = false;
            this.btnDangNhap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDangNhap.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnDangNhap.Location = new System.Drawing.Point(56, 20);
            this.btnDangNhap.MauNen = System.Drawing.Color.FromArgb(11, 31, 51);
            this.btnDangNhap.MauRe = System.Drawing.Color.FromArgb(23, 71, 196);
            this.btnDangNhap.MauChu = System.Drawing.Color.White;
            this.btnDangNhap.Name = "btnDangNhap";
            this.btnDangNhap.Size = new System.Drawing.Size(448, 58);
            this.btnDangNhap.TabIndex = 4;
            this.btnDangNhap.Text = "Đăng nhập";
            this.btnDangNhap.UseVisualStyleBackColor = false;
            this.btnDangNhap.Click += new System.EventHandler(this.btnDangNhap_Click);
            //
            // pnlHoacTrai / lblHoac / pnlHoacPhai (dong "Hoac dang nhap nhanh")
            //
            this.pnlHoacTrai.BackColor = System.Drawing.Color.FromArgb(214, 224, 238);
            this.pnlHoacTrai.Location = new System.Drawing.Point(56, 94);
            this.pnlHoacTrai.Name = "pnlHoacTrai";
            this.pnlHoacTrai.Size = new System.Drawing.Size(90, 1);
            this.pnlHoacTrai.TabIndex = 29;
            //
            this.lblHoac.AutoSize = false;
            this.lblHoac.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblHoac.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblHoac.Location = new System.Drawing.Point(150, 84);
            this.lblHoac.Name = "lblHoac";
            this.lblHoac.Size = new System.Drawing.Size(260, 20);
            this.lblHoac.TabIndex = 30;
            this.lblHoac.Text = "Hoặc đăng nhập nhanh";
            this.lblHoac.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            this.pnlHoacPhai.BackColor = System.Drawing.Color.FromArgb(214, 224, 238);
            this.pnlHoacPhai.Location = new System.Drawing.Point(414, 94);
            this.pnlHoacPhai.Name = "pnlHoacPhai";
            this.pnlHoacPhai.Size = new System.Drawing.Size(90, 1);
            this.pnlHoacPhai.TabIndex = 31;
            //
            // btnVssId (chuc nang mo rong)
            //
            this.btnVssId.BanKinh = 8;
            this.btnVssId.KieuVien = true;
            this.btnVssId.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVssId.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.btnVssId.Location = new System.Drawing.Point(56, 108);
            this.btnVssId.MauNen = System.Drawing.Color.FromArgb(23, 71, 196);
            this.btnVssId.Name = "btnVssId";
            this.btnVssId.Size = new System.Drawing.Size(218, 48);
            this.btnVssId.TabIndex = 5;
            this.btnVssId.Text = "VssID";
            this.btnVssId.UseVisualStyleBackColor = false;
            this.btnVssId.Click += new System.EventHandler(this.btnVssId_Click);
            //
            // btnGoogle (chuc nang mo rong)
            //
            this.btnGoogle.BanKinh = 8;
            this.btnGoogle.KieuVien = true;
            this.btnGoogle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGoogle.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.btnGoogle.Location = new System.Drawing.Point(286, 108);
            this.btnGoogle.MauNen = System.Drawing.Color.FromArgb(23, 71, 196);
            this.btnGoogle.Name = "btnGoogle";
            this.btnGoogle.Size = new System.Drawing.Size(218, 48);
            this.btnGoogle.TabIndex = 6;
            this.btnGoogle.Text = "Google";
            this.btnGoogle.UseVisualStyleBackColor = false;
            this.btnGoogle.Click += new System.EventHandler(this.btnGoogle_Click);
            //
            // lnkChuyenDangKy: "Không có tài khoản? Đăng ký" (căn giữa, chỉ chữ "Đăng ký" là liên kết)
            //
            this.lnkChuyenDangKy.ActiveLinkColor = System.Drawing.Color.FromArgb(12, 38, 128);
            this.lnkChuyenDangKy.AutoSize = false;
            this.lnkChuyenDangKy.Cursor = System.Windows.Forms.Cursors.Default;
            this.lnkChuyenDangKy.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lnkChuyenDangKy.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lnkChuyenDangKy.LinkArea = new System.Windows.Forms.LinkArea(20, 7);
            this.lnkChuyenDangKy.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkChuyenDangKy.LinkColor = System.Drawing.Color.FromArgb(23, 71, 196);
            this.lnkChuyenDangKy.Location = new System.Drawing.Point(56, 168);
            this.lnkChuyenDangKy.Name = "lnkChuyenDangKy";
            this.lnkChuyenDangKy.Size = new System.Drawing.Size(448, 32);
            this.lnkChuyenDangKy.TabIndex = 7;
            this.lnkChuyenDangKy.TabStop = true;
            this.lnkChuyenDangKy.Text = "Không có tài khoản? Đăng ký";
            this.lnkChuyenDangKy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkChuyenDangKy.VisitedLinkColor = System.Drawing.Color.FromArgb(23, 71, 196);
            this.lnkChuyenDangKy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkChuyenDangKy_LinkClicked);
            //
            // Gom control vao vung
            //
            this.pnlCuon.Controls.Add(this.lnkQuenMatKhau);
            this.pnlCuon.Controls.Add(this.chkNhoTaiKhoan);
            this.pnlCuon.Controls.Add(this.lblLoiMatKhau);
            this.pnlCuon.Controls.Add(this.txtMatKhau);
            this.pnlCuon.Controls.Add(this.lblMatKhau);
            this.pnlCuon.Controls.Add(this.lblLoiTenDangNhap);
            this.pnlCuon.Controls.Add(this.txtTenDangNhap);
            this.pnlCuon.Controls.Add(this.lblTenDangNhap);
            this.pnlCuon.Controls.Add(this.lblTieuDe);
            this.pnlCuon.Controls.Add(this.huyHieu);
            this.pnlHanhDong.Controls.Add(this.lnkChuyenDangKy);
            this.pnlHanhDong.Controls.Add(this.btnGoogle);
            this.pnlHanhDong.Controls.Add(this.btnVssId);
            this.pnlHanhDong.Controls.Add(this.pnlHoacPhai);
            this.pnlHanhDong.Controls.Add(this.lblHoac);
            this.pnlHanhDong.Controls.Add(this.pnlHoacTrai);
            this.pnlHanhDong.Controls.Add(this.btnDangNhap);
            this.pnlHanhDong.Controls.Add(this.lblLoiChung);
            // Thu tu: Fill (chi so 0) -> vung tren nut -> canh bao (sat day) -> tab (sat dinh)
            this.pnlNoiDung.Controls.Add(this.pnlCuon);
            this.pnlNoiDung.Controls.Add(this.pnlHanhDong);
            this.pnlNoiDung.Controls.Add(this.canhBaoYTe);
            this.pnlNoiDung.Controls.Add(this.thanhTab);
            //
            // FrmDangNhap
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AcceptButton = this.btnDangNhap;
            this.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.ClientSize = new System.Drawing.Size(920, 660);
            this.Controls.Add(this.pnlNoiDung);
            this.Controls.Add(this.pnlThuongHieu);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmDangNhap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hệ thống tra cứu sức khỏe";
            this.pnlHanhDong.ResumeLayout(false);
            this.pnlCuon.ResumeLayout(false);
            this.pnlNoiDung.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private HealthClient.UI.PanelThuongHieu pnlThuongHieu;
        private System.Windows.Forms.Panel pnlNoiDung;
        private System.Windows.Forms.Panel pnlCuon;
        private System.Windows.Forms.Panel pnlHanhDong;
        private HealthClient.UI.ThanhTab thanhTab;
        private HealthClient.UI.CanhBaoYTe canhBaoYTe;
        private HealthClient.UI.HuyHieu huyHieu;
        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblTenDangNhap;
        private HealthClient.UI.OnhapLieu txtTenDangNhap;
        private System.Windows.Forms.Label lblLoiTenDangNhap;
        private System.Windows.Forms.Label lblMatKhau;
        private HealthClient.UI.OnhapLieu txtMatKhau;
        private System.Windows.Forms.Label lblLoiMatKhau;
        private HealthClient.UI.HopChon chkNhoTaiKhoan;
        private System.Windows.Forms.LinkLabel lnkQuenMatKhau;
        private System.Windows.Forms.Label lblLoiChung;
        private HealthClient.UI.NutBoGoc btnDangNhap;
        private System.Windows.Forms.Panel pnlHoacTrai;
        private System.Windows.Forms.Label lblHoac;
        private System.Windows.Forms.Panel pnlHoacPhai;
        private HealthClient.UI.NutBoGoc btnVssId;
        private HealthClient.UI.NutBoGoc btnGoogle;
        private System.Windows.Forms.LinkLabel lnkChuyenDangKy;
    }
}
