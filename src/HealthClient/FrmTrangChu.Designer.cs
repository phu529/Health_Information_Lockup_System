#nullable disable
using System.Drawing;
using System.Windows.Forms;

namespace HealthClient
{
    partial class FrmTrangChu
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

        private void InitializeComponent()
        {
            pnlTieuDe = new Panel();
            lblTieuDe = new Label();
            lblXinChao = new Label();
            lblGhiChu = new Label();
            btnDangXuat = new HealthClient.UI.NutBoGoc();
            canhBaoYTe = new HealthClient.UI.CanhBaoYTe();
            pnlTieuDe.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTieuDe
            // 
            pnlTieuDe.BackColor = Color.FromArgb(23, 71, 196);
            pnlTieuDe.Controls.Add(lblTieuDe);
            pnlTieuDe.Dock = DockStyle.Top;
            pnlTieuDe.Location = new Point(0, 0);
            pnlTieuDe.Name = "pnlTieuDe";
            pnlTieuDe.Size = new Size(720, 64);
            pnlTieuDe.TabIndex = 4;
            // 
            // lblTieuDe
            // 
            lblTieuDe.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTieuDe.ForeColor = Color.White;
            lblTieuDe.Location = new Point(28, 12);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(660, 40);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "Hệ thống tra cứu triệu chứng";
            lblTieuDe.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblXinChao
            // 
            lblXinChao.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblXinChao.ForeColor = Color.FromArgb(11, 31, 51);
            lblXinChao.Location = new Point(28, 96);
            lblXinChao.Name = "lblXinChao";
            lblXinChao.Size = new Size(664, 44);
            lblXinChao.TabIndex = 1;
            lblXinChao.Text = "Xin chào,";
            lblXinChao.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGhiChu
            // 
            lblGhiChu.Font = new Font("Segoe UI", 12.5F);
            lblGhiChu.ForeColor = Color.FromArgb(75, 85, 99);
            lblGhiChu.Location = new Point(28, 148);
            lblGhiChu.Name = "lblGhiChu";
            lblGhiChu.Size = new Size(664, 28);
            lblGhiChu.TabIndex = 2;
            lblGhiChu.Text = "Màn hình tra cứu sẽ được làm ở Bước 1.4.";
            lblGhiChu.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnDangXuat
            // 
            btnDangXuat.BanKinh = 8;
            btnDangXuat.Cursor = Cursors.Hand;
            btnDangXuat.FlatStyle = FlatStyle.Flat;
            btnDangXuat.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnDangXuat.KieuVien = false;
            btnDangXuat.Location = new Point(28, 200);
            btnDangXuat.MauChu = Color.White;
            btnDangXuat.MauNen = Color.FromArgb(11, 31, 51);
            btnDangXuat.MauRe = Color.FromArgb(23, 71, 196);
            btnDangXuat.Name = "btnDangXuat";
            btnDangXuat.Size = new Size(200, 64);
            btnDangXuat.TabIndex = 0;
            btnDangXuat.Text = "Đăng xuất";
            btnDangXuat.UseVisualStyleBackColor = false;
            btnDangXuat.Click += btnDangXuat_Click;
            // 
            // canhBaoYTe
            // 
            canhBaoYTe.AccessibleName = "Cảnh báo y khoa";
            canhBaoYTe.AccessibleRole = AccessibleRole.Alert;
            canhBaoYTe.Dock = DockStyle.Bottom;
            canhBaoYTe.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            canhBaoYTe.Location = new Point(0, 292);
            canhBaoYTe.Name = "canhBaoYTe";
            canhBaoYTe.Size = new Size(720, 93);
            canhBaoYTe.TabIndex = 3;
            canhBaoYTe.TabStop = false;
            canhBaoYTe.Text = "Thông tin chỉ mang tính tham khảo, không thể thay thế chẩn đoán của bác sĩ, nếu vấn đề của bạn quan trọng hãy đến các bệnh viện gần nhất";
            // 
            // FrmTrangChu
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(720, 385);
            Controls.Add(canhBaoYTe);
            Controls.Add(btnDangXuat);
            Controls.Add(lblGhiChu);
            Controls.Add(lblXinChao);
            Controls.Add(pnlTieuDe);
            Font = new Font("Segoe UI", 12F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FrmTrangChu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Trang chính — Hệ thống tra cứu triệu chứng";
            pnlTieuDe.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel pnlTieuDe;
        private Label lblTieuDe;
        private Label lblXinChao;
        private Label lblGhiChu;
        private HealthClient.UI.NutBoGoc btnDangXuat;
        private HealthClient.UI.CanhBaoYTe canhBaoYTe;
    }
}
