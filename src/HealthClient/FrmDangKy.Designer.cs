#nullable disable
using System.Drawing;
using System.Windows.Forms;

namespace HealthClient
{
    partial class FrmDangKy
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

        // v17: cung khung Dock voi form Dang nhap:
        //   thanhTab (Top) | canhBaoYTe (Bottom, sat day) | pnlHanhDong (Bottom, nut Dang ky / Huy) | pnlCuon (Fill, cuon duoc)
        // Gach chan xanh do ThanhTab tu ve nen luon nam duoi chu "Dang ky" (v16 bi nhan tieu de de len).
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
            this.lblHoTen = new System.Windows.Forms.Label();
            this.txtHoTen = new HealthClient.UI.OnhapLieu();
            this.lblLoiHoTen = new System.Windows.Forms.Label();
            this.lblTenDangNhap = new System.Windows.Forms.Label();
            this.txtTenDangNhap = new HealthClient.UI.OnhapLieu();
            this.lblLoiTenDangNhap = new System.Windows.Forms.Label();
            this.lblMatKhau = new System.Windows.Forms.Label();
            this.txtMatKhau = new HealthClient.UI.OnhapLieu();
            this.lblLoiMatKhau = new System.Windows.Forms.Label();
            this.lblXacNhanMatKhau = new System.Windows.Forms.Label();
            this.txtXacNhanMatKhau = new HealthClient.UI.OnhapLieu();
            this.lblLoiXacNhanMatKhau = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new HealthClient.UI.OnhapLieu();
            this.lblLoiEmail = new System.Windows.Forms.Label();
            this.lblLoiChung = new System.Windows.Forms.Label();
            this.btnDangKy = new HealthClient.UI.NutBoGoc();
            this.btnHuy = new HealthClient.UI.NutBoGoc();
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
            // thanhTab (tab Dang ky dang chon: gach xanh nam duoi chu "Dang ky")
            //
            this.thanhTab.Dock = System.Windows.Forms.DockStyle.Top;
            this.thanhTab.Location = new System.Drawing.Point(0, 0);
            this.thanhTab.Name = "thanhTab";
            this.thanhTab.Size = new System.Drawing.Size(560, 60);
            this.thanhTab.TabDangChon = 1;
            this.thanhTab.ChiHienTab = 1;
            this.thanhTab.TabIndex = 40;
            this.thanhTab.TabBam += new System.EventHandler<int>(this.thanhTab_TabBam);
            //
            // canhBaoYTe (Dock Bottom, tu tinh chieu cao theo chu)
            //
            this.canhBaoYTe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.canhBaoYTe.Location = new System.Drawing.Point(0, 580);
            this.canhBaoYTe.Name = "canhBaoYTe";
            this.canhBaoYTe.Size = new System.Drawing.Size(560, 80);
            this.canhBaoYTe.TabIndex = 41;
            //
            // pnlHanhDong
            //
            this.pnlHanhDong.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.pnlHanhDong.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHanhDong.Location = new System.Drawing.Point(0, 494);
            this.pnlHanhDong.Name = "pnlHanhDong";
            this.pnlHanhDong.Size = new System.Drawing.Size(560, 86);
            this.pnlHanhDong.TabIndex = 1;
            //
            // pnlCuon
            //
            this.pnlCuon.AutoScroll = true;
            this.pnlCuon.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.pnlCuon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCuon.Location = new System.Drawing.Point(0, 60);
            this.pnlCuon.Name = "pnlCuon";
            this.pnlCuon.Size = new System.Drawing.Size(560, 434);
            this.pnlCuon.TabIndex = 0;
            //
            // huyHieu
            //
            this.huyHieu.Location = new System.Drawing.Point(56, 10);
            this.huyHieu.MauCham = System.Drawing.Color.FromArgb(23, 71, 196);
            this.huyHieu.Name = "huyHieu";
            this.huyHieu.Size = new System.Drawing.Size(320, 28);
            this.huyHieu.TabIndex = 42;
            this.huyHieu.Text = "Chỉ mất một phút để bắt đầu";
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = false;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(11, 31, 51);
            this.lblTieuDe.Location = new System.Drawing.Point(56, 42);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(448, 40);
            this.lblTieuDe.TabIndex = 43;
            this.lblTieuDe.Text = "Tạo tài khoản mới";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblHoTen / txtHoTen / lblLoiHoTen
            //
            this.lblHoTen.AutoSize = false;
            this.lblHoTen.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHoTen.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblHoTen.Location = new System.Drawing.Point(56, 90);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(448, 22);
            this.lblHoTen.TabIndex = 30;
            this.lblHoTen.Text = "Họ và tên";
            this.lblHoTen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            this.txtHoTen.BieuTuong = HealthClient.UI.BieuTuongO.NguoiDung;
            this.txtHoTen.CoNutHienAn = false;
            this.txtHoTen.Location = new System.Drawing.Point(56, 112);
            this.txtHoTen.MaxLength = 50;
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.PlaceholderText = "Ví dụ: Nguyễn Văn A";
            this.txtHoTen.Size = new System.Drawing.Size(448, 44);
            this.txtHoTen.TabIndex = 0;
            //
            this.lblLoiHoTen.AutoSize = false;
            this.lblLoiHoTen.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiHoTen.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiHoTen.Location = new System.Drawing.Point(56, 158);
            this.lblLoiHoTen.Name = "lblLoiHoTen";
            this.lblLoiHoTen.Size = new System.Drawing.Size(448, 18);
            this.lblLoiHoTen.TabIndex = 31;
            this.lblLoiHoTen.Text = "";
            this.lblLoiHoTen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblTenDangNhap / txtTenDangNhap / lblLoiTenDangNhap
            //
            this.lblTenDangNhap.AutoSize = false;
            this.lblTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTenDangNhap.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblTenDangNhap.Location = new System.Drawing.Point(56, 178);
            this.lblTenDangNhap.Name = "lblTenDangNhap";
            this.lblTenDangNhap.Size = new System.Drawing.Size(448, 22);
            this.lblTenDangNhap.TabIndex = 33;
            this.lblTenDangNhap.Text = "Tên đăng nhập";
            this.lblTenDangNhap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            this.txtTenDangNhap.BieuTuong = HealthClient.UI.BieuTuongO.NguoiDung;
            this.txtTenDangNhap.CoNutHienAn = false;
            this.txtTenDangNhap.Location = new System.Drawing.Point(56, 200);
            this.txtTenDangNhap.MaxLength = 20;
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.PlaceholderText = "3-20 ký tự, không dấu";
            this.txtTenDangNhap.Size = new System.Drawing.Size(448, 44);
            this.txtTenDangNhap.TabIndex = 1;
            //
            this.lblLoiTenDangNhap.AutoSize = false;
            this.lblLoiTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiTenDangNhap.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiTenDangNhap.Location = new System.Drawing.Point(56, 246);
            this.lblLoiTenDangNhap.Name = "lblLoiTenDangNhap";
            this.lblLoiTenDangNhap.Size = new System.Drawing.Size(448, 18);
            this.lblLoiTenDangNhap.TabIndex = 34;
            this.lblLoiTenDangNhap.Text = "";
            this.lblLoiTenDangNhap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblMatKhau / txtMatKhau / lblLoiMatKhau
            //
            this.lblMatKhau.AutoSize = false;
            this.lblMatKhau.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblMatKhau.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblMatKhau.Location = new System.Drawing.Point(56, 266);
            this.lblMatKhau.Name = "lblMatKhau";
            this.lblMatKhau.Size = new System.Drawing.Size(448, 22);
            this.lblMatKhau.TabIndex = 36;
            this.lblMatKhau.Text = "Mật khẩu";
            this.lblMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            this.txtMatKhau.BieuTuong = HealthClient.UI.BieuTuongO.KhoaMatKhau;
            this.txtMatKhau.CoNutHienAn = true;
            this.txtMatKhau.Location = new System.Drawing.Point(56, 288);
            this.txtMatKhau.MaxLength = 64;
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.PlaceholderText = "Tối thiểu 8 ký tự, có chữ và số";
            this.txtMatKhau.Size = new System.Drawing.Size(448, 44);
            this.txtMatKhau.TabIndex = 2;
            this.txtMatKhau.UseSystemPasswordChar = true;
            //
            this.lblLoiMatKhau.AutoSize = false;
            this.lblLoiMatKhau.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiMatKhau.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiMatKhau.Location = new System.Drawing.Point(56, 334);
            this.lblLoiMatKhau.Name = "lblLoiMatKhau";
            this.lblLoiMatKhau.Size = new System.Drawing.Size(448, 18);
            this.lblLoiMatKhau.TabIndex = 37;
            this.lblLoiMatKhau.Text = "";
            this.lblLoiMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblXacNhanMatKhau / txtXacNhanMatKhau / lblLoiXacNhanMatKhau
            //
            this.lblXacNhanMatKhau.AutoSize = false;
            this.lblXacNhanMatKhau.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblXacNhanMatKhau.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblXacNhanMatKhau.Location = new System.Drawing.Point(56, 354);
            this.lblXacNhanMatKhau.Name = "lblXacNhanMatKhau";
            this.lblXacNhanMatKhau.Size = new System.Drawing.Size(448, 22);
            this.lblXacNhanMatKhau.TabIndex = 39;
            this.lblXacNhanMatKhau.Text = "Nhập lại mật khẩu";
            this.lblXacNhanMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            this.txtXacNhanMatKhau.BieuTuong = HealthClient.UI.BieuTuongO.KhoaMatKhau;
            this.txtXacNhanMatKhau.CoNutHienAn = true;
            this.txtXacNhanMatKhau.Location = new System.Drawing.Point(56, 376);
            this.txtXacNhanMatKhau.MaxLength = 64;
            this.txtXacNhanMatKhau.Name = "txtXacNhanMatKhau";
            this.txtXacNhanMatKhau.PlaceholderText = "Nhập lại mật khẩu ở trên";
            this.txtXacNhanMatKhau.Size = new System.Drawing.Size(448, 44);
            this.txtXacNhanMatKhau.TabIndex = 3;
            this.txtXacNhanMatKhau.UseSystemPasswordChar = true;
            //
            this.lblLoiXacNhanMatKhau.AutoSize = false;
            this.lblLoiXacNhanMatKhau.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiXacNhanMatKhau.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiXacNhanMatKhau.Location = new System.Drawing.Point(56, 422);
            this.lblLoiXacNhanMatKhau.Name = "lblLoiXacNhanMatKhau";
            this.lblLoiXacNhanMatKhau.Size = new System.Drawing.Size(448, 18);
            this.lblLoiXacNhanMatKhau.TabIndex = 40;
            this.lblLoiXacNhanMatKhau.Text = "";
            this.lblLoiXacNhanMatKhau.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblEmail / txtEmail / lblLoiEmail
            //
            this.lblEmail.AutoSize = false;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblEmail.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.lblEmail.Location = new System.Drawing.Point(56, 442);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(448, 22);
            this.lblEmail.TabIndex = 42;
            this.lblEmail.Text = "Email (không bắt buộc)";
            this.lblEmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            this.txtEmail.BieuTuong = HealthClient.UI.BieuTuongO.ThuDienTu;
            this.txtEmail.CoNutHienAn = false;
            this.txtEmail.Location = new System.Drawing.Point(56, 464);
            this.txtEmail.MaxLength = 100;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderText = "ten@mien.com";
            this.txtEmail.Size = new System.Drawing.Size(448, 44);
            this.txtEmail.TabIndex = 4;
            //
            this.lblLoiEmail.AutoSize = false;
            this.lblLoiEmail.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiEmail.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiEmail.Location = new System.Drawing.Point(56, 510);
            this.lblLoiEmail.Name = "lblLoiEmail";
            this.lblLoiEmail.Size = new System.Drawing.Size(448, 18);
            this.lblLoiEmail.TabIndex = 43;
            this.lblLoiEmail.Text = "";
            this.lblLoiEmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblLoiChung (nam tren hai nut)
            //
            this.lblLoiChung.AutoSize = false;
            this.lblLoiChung.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLoiChung.ForeColor = System.Drawing.Color.FromArgb(180, 35, 24);
            this.lblLoiChung.Location = new System.Drawing.Point(56, 0);
            this.lblLoiChung.Name = "lblLoiChung";
            this.lblLoiChung.Size = new System.Drawing.Size(448, 20);
            this.lblLoiChung.TabIndex = 50;
            this.lblLoiChung.Text = "";
            this.lblLoiChung.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnDangKy (nut chinh: navy, re chuot doi sang xanh)
            //
            this.btnDangKy.BanKinh = 8;
            this.btnDangKy.KieuVien = false;
            this.btnDangKy.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDangKy.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnDangKy.Location = new System.Drawing.Point(56, 22);
            this.btnDangKy.MauNen = System.Drawing.Color.FromArgb(11, 31, 51);
            this.btnDangKy.MauRe = System.Drawing.Color.FromArgb(23, 71, 196);
            this.btnDangKy.MauChu = System.Drawing.Color.White;
            this.btnDangKy.Name = "btnDangKy";
            this.btnDangKy.Size = new System.Drawing.Size(300, 58);
            this.btnDangKy.TabIndex = 5;
            this.btnDangKy.Text = "Đăng ký";
            this.btnDangKy.UseVisualStyleBackColor = false;
            this.btnDangKy.Click += new System.EventHandler(this.btnDangKy_Click);
            //
            // btnHuy
            //
            this.btnHuy.BanKinh = 8;
            this.btnHuy.KieuVien = true;
            this.btnHuy.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnHuy.Location = new System.Drawing.Point(372, 22);
            this.btnHuy.MauNen = System.Drawing.Color.FromArgb(23, 71, 196);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(132, 58);
            this.btnHuy.TabIndex = 6;
            this.btnHuy.Text = "Huỷ";
            this.btnHuy.UseVisualStyleBackColor = false;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            //
            // Gom control vao vung
            //
            this.pnlCuon.Controls.Add(this.lblLoiEmail);
            this.pnlCuon.Controls.Add(this.txtEmail);
            this.pnlCuon.Controls.Add(this.lblEmail);
            this.pnlCuon.Controls.Add(this.lblLoiXacNhanMatKhau);
            this.pnlCuon.Controls.Add(this.txtXacNhanMatKhau);
            this.pnlCuon.Controls.Add(this.lblXacNhanMatKhau);
            this.pnlCuon.Controls.Add(this.lblLoiMatKhau);
            this.pnlCuon.Controls.Add(this.txtMatKhau);
            this.pnlCuon.Controls.Add(this.lblMatKhau);
            this.pnlCuon.Controls.Add(this.lblLoiTenDangNhap);
            this.pnlCuon.Controls.Add(this.txtTenDangNhap);
            this.pnlCuon.Controls.Add(this.lblTenDangNhap);
            this.pnlCuon.Controls.Add(this.lblLoiHoTen);
            this.pnlCuon.Controls.Add(this.txtHoTen);
            this.pnlCuon.Controls.Add(this.lblHoTen);
            this.pnlCuon.Controls.Add(this.lblTieuDe);
            this.pnlCuon.Controls.Add(this.huyHieu);
            this.pnlHanhDong.Controls.Add(this.btnHuy);
            this.pnlHanhDong.Controls.Add(this.btnDangKy);
            this.pnlHanhDong.Controls.Add(this.lblLoiChung);
            // Thu tu: Fill (chi so 0) -> vung nut -> canh bao (sat day) -> tab (sat dinh)
            this.pnlNoiDung.Controls.Add(this.pnlCuon);
            this.pnlNoiDung.Controls.Add(this.pnlHanhDong);
            this.pnlNoiDung.Controls.Add(this.canhBaoYTe);
            this.pnlNoiDung.Controls.Add(this.thanhTab);
            //
            // FrmDangKy
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AcceptButton = this.btnDangKy;
            this.BackColor = System.Drawing.Color.FromArgb(250, 252, 255);
            this.CancelButton = this.btnHuy;
            this.ClientSize = new System.Drawing.Size(920, 660);
            this.Controls.Add(this.pnlNoiDung);
            this.Controls.Add(this.pnlThuongHieu);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmDangKy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hệ thống tra cứu sức khỏe - Đăng ký";
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
        private System.Windows.Forms.Label lblHoTen;
        private HealthClient.UI.OnhapLieu txtHoTen;
        private System.Windows.Forms.Label lblLoiHoTen;
        private System.Windows.Forms.Label lblTenDangNhap;
        private HealthClient.UI.OnhapLieu txtTenDangNhap;
        private System.Windows.Forms.Label lblLoiTenDangNhap;
        private System.Windows.Forms.Label lblMatKhau;
        private HealthClient.UI.OnhapLieu txtMatKhau;
        private System.Windows.Forms.Label lblLoiMatKhau;
        private System.Windows.Forms.Label lblXacNhanMatKhau;
        private HealthClient.UI.OnhapLieu txtXacNhanMatKhau;
        private System.Windows.Forms.Label lblLoiXacNhanMatKhau;
        private System.Windows.Forms.Label lblEmail;
        private HealthClient.UI.OnhapLieu txtEmail;
        private System.Windows.Forms.Label lblLoiEmail;
        private System.Windows.Forms.Label lblLoiChung;
        private HealthClient.UI.NutBoGoc btnDangKy;
        private HealthClient.UI.NutBoGoc btnHuy;
    }
}
