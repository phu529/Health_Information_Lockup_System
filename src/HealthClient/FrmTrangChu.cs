namespace HealthClient
{
    /// <summary>
    /// Man hinh TAM sau khi dang nhap thanh cong. Buoc 1.4 se thay bang
    /// giao dien tra cuu day du (tim kiem, nhom co quan, lich su...).
    /// </summary>
    public partial class FrmTrangChu : Form
    {
        // Constructor khong tham so de Designer mo duoc form.
        public FrmTrangChu()
        {
            InitializeComponent();
        }

        public FrmTrangChu(string hoTen) : this()
        {
            lblXinChao.Text = $"Xin chào, {hoTen}";
        }

        private void btnDangXuat_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
