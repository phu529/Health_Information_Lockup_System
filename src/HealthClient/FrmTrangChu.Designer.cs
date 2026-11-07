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
            this.pnlTieuDe = new System.Windows.Forms.Panel();
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblXinChao = new System.Windows.Forms.Label();
            this.lblGhiChu = new System.Windows.Forms.Label();
            this.btnDangXuat = new HealthClient.UI.NutBoGoc();
            this.canhBaoYTe = new HealthClient.UI.CanhBaoYTe();
            this.pnlTieuDe.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTieuDe
            //
            this.pnlTieuDe.BackColor = System.Drawing.Color.FromArgb(23, 71, 196);
            this.pnlTieuDe.Controls.Add(this.lblTieuDe);
            this.pnlTieuDe.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTieuDe.Location = new System.Drawing.Point(0, 0);
            this.pnlTieuDe.Name = "pnlTieuDe";
            this.pnlTieuDe.Size = new System.Drawing.Size(720, 64);
            this.pnlTieuDe.TabIndex = 4;
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = false;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.White;
            this.lblTieuDe.Location = new System.Drawing.Point(28, 12);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(660, 40);
            this.lblTieuDe.TabIndex = 0;
            this.lblTieuDe.Text = "Hệ thống tra cứu triệu chứng";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblXinChao
            //
            this.lblXinChao.AutoSize = false;
            this.lblXinChao.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblXinChao.ForeColor = System.Drawing.Color.FromArgb(11, 31, 51);
            this.lblXinChao.Location = new System.Drawing.Point(28, 96);
            this.lblXinChao.Name = "lblXinChao";
            this.lblXinChao.Size = new System.Drawing.Size(664, 44);
            this.lblXinChao.TabIndex = 1;
            this.lblXinChao.Text = "Xin chào,";
            this.lblXinChao.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblGhiChu
            //
            this.lblGhiChu.AutoSize = false;
            this.lblGhiChu.Font = new System.Drawing.Font("Segoe UI", 12.5F);
            this.lblGhiChu.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblGhiChu.Location = new System.Drawing.Point(28, 148);
            this.lblGhiChu.Name = "lblGhiChu";
            this.lblGhiChu.Size = new System.Drawing.Size(664, 28);
            this.lblGhiChu.TabIndex = 2;
            this.lblGhiChu.Text = "Màn hình tra cứu sẽ được làm ở Bước 1.4.";
            this.lblGhiChu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnDangXuat
            //
            this.btnDangXuat.BanKinh = 8;
            this.btnDangXuat.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDangXuat.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnDangXuat.KieuVien = false;
            this.btnDangXuat.Location = new System.Drawing.Point(28, 200);
            this.btnDangXuat.MauNen = System.Drawing.Color.FromArgb(11, 31, 51);
            this.btnDangXuat.MauRe = System.Drawing.Color.FromArgb(23, 71, 196);
            this.btnDangXuat.MauChu = System.Drawing.Color.White;
            this.btnDangXuat.Name = "btnDangXuat";
            this.btnDangXuat.Size = new System.Drawing.Size(200, 64);
            this.btnDangXuat.TabIndex = 0;
            this.btnDangXuat.Text = "Đăng xuất";
            this.btnDangXuat.UseVisualStyleBackColor = false;
            this.btnDangXuat.Click += new System.EventHandler(this.btnDangXuat_Click);
            //
            // canhBaoYTe (Dock Bottom, tu tinh chieu cao theo chu; cau canh bao theo de tai)
            //
            this.canhBaoYTe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.canhBaoYTe.Location = new System.Drawing.Point(0, 290);
            this.canhBaoYTe.Name = "canhBaoYTe";
            this.canhBaoYTe.Size = new System.Drawing.Size(720, 90);
            this.canhBaoYTe.TabIndex = 3;
            //
            // FrmTrangChu
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(720, 380);
            this.Controls.Add(this.canhBaoYTe);
            this.Controls.Add(this.btnDangXuat);
            this.Controls.Add(this.lblGhiChu);
            this.Controls.Add(this.lblXinChao);
            this.Controls.Add(this.pnlTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmTrangChu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trang chính — Hệ thống tra cứu triệu chứng";
            this.pnlTieuDe.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Panel pnlTieuDe;
        private Label lblTieuDe;
        private Label lblXinChao;
        private Label lblGhiChu;
        private HealthClient.UI.NutBoGoc btnDangXuat;
        private HealthClient.UI.CanhBaoYTe canhBaoYTe;
    }
}
