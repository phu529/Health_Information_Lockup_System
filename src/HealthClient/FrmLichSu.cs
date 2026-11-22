using HealthClient.Services;
using HealthShared;

namespace HealthClient;

public sealed class FrmLichSu : Form
{
    private readonly UserProfileHistoryService _service = new();
    private List<SearchHistoryItemPayload> _danhSach = [];

    private DataGridView dgvLichSu = null!;
    private Label lblTrangThai = null!;
    private Button btnLamMoi = null!;
    private Button btnTraCuuLai = null!;
    private Button btnXoaTatCa = null!;
    private Button btnDong = null!;

    public string? TuKhoaChonLai { get; private set; }

    public FrmLichSu()
    {
        KhoiTaoGiaoDien();
        Load += async (_, _) => await TaiLichSuAsync();
    }

    private void KhoiTaoGiaoDien()
    {
        Text = "Lịch sử tra cứu";
        ClientSize = new Size(720, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);

        var lblTieuDe = new Label
        {
            Text = "LỊCH SỬ TRA CỨU Y TẾ",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 118, 110),
            Location = new Point(20, 16),
            AutoSize = true
        };

        var lblGoiY = new Label
        {
            Text = "Nhấp đúp vào một dòng (hoặc bấm 'Tra cứu lại') để tìm lại từ khóa đó.",
            ForeColor = Color.DimGray,
            Location = new Point(22, 44),
            AutoSize = true
        };

        dgvLichSu = new DataGridView
        {
            Location = new Point(20, 74),
            Size = new Size(680, 320),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ThoiGian",
            HeaderText = "Thời gian tra cứu",
            FillWeight = 24
        });
        dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TuKhoa",
            HeaderText = "Từ khóa / Nội dung",
            FillWeight = 34
        });
        dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TenBenh",
            HeaderText = "Bệnh đã xem",
            FillWeight = 28
        });
        dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SoKetQua",
            HeaderText = "Số kết quả",
            FillWeight = 14
        });

        dgvLichSu.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                ChonTuKhoaVaDong();
        };

        lblTrangThai = new Label
        {
            Text = "Đang tải...",
            ForeColor = Color.DimGray,
            Location = new Point(20, 404),
            Size = new Size(680, 20)
        };

        btnLamMoi = new Button
        {
            Text = "Làm mới",
            Location = new Point(20, 432),
            Size = new Size(95, 34),
            UseVisualStyleBackColor = true,
            Cursor = Cursors.Hand
        };
        btnLamMoi.Click += async (_, _) => await TaiLichSuAsync();

        btnTraCuuLai = new Button
        {
            Text = "Tra cứu lại",
            Location = new Point(125, 432),
            Size = new Size(110, 34),
            BackColor = Color.FromArgb(15, 118, 110),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnTraCuuLai.FlatAppearance.BorderSize = 0;
        btnTraCuuLai.Click += (_, _) => ChonTuKhoaVaDong();

        btnXoaTatCa = new Button
        {
            Text = "Xóa tất cả lịch sử",
            Location = new Point(455, 432),
            Size = new Size(140, 34),
            BackColor = Color.FromArgb(220, 38, 38),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnXoaTatCa.FlatAppearance.BorderSize = 0;
        btnXoaTatCa.Click += async (_, _) => await XoaTatCaAsync();

        btnDong = new Button
        {
            Text = "Đóng",
            Location = new Point(605, 432),
            Size = new Size(95, 34),
            UseVisualStyleBackColor = true,
            Cursor = Cursors.Hand
        };
        btnDong.Click += (_, _) => Close();

        Controls.AddRange([
            lblTieuDe, lblGoiY, dgvLichSu, lblTrangThai,
            btnLamMoi, btnTraCuuLai, btnXoaTatCa, btnDong
        ]);
    }

    private async Task TaiLichSuAsync()
    {
        SetDangXuLy(true, "Đang tải lịch sử tra cứu...");
        var ketQua = await _service.LayLichSuAsync(100);
        SetDangXuLy(false);

        if (!ketQua.ThanhCong || ketQua.DuLieu is null)
        {
            lblTrangThai.Text = ketQua.ThongBao;
            MessageBox.Show(ketQua.ThongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _danhSach = ketQua.DuLieu;
        dgvLichSu.Rows.Clear();

        foreach (var item in _danhSach)
        {
            DateTime localTime = DateTime.SpecifyKind(item.ThoiGianTim, DateTimeKind.Utc).ToLocalTime();
            dgvLichSu.Rows.Add(
                localTime.ToString("dd/MM/yyyy HH:mm:ss"),
                item.TuKhoa,
                string.IsNullOrWhiteSpace(item.TenBenh) ? "--" : item.TenBenh,
                item.SoKetQua);
        }

        lblTrangThai.Text = _danhSach.Count == 0
            ? "Bạn chưa có lịch sử tra cứu nào."
            : $"Hiển thị {_danhSach.Count} lượt tra cứu gần nhất.";
    }

    private void ChonTuKhoaVaDong()
    {
        if (dgvLichSu.CurrentRow is null)
        {
            MessageBox.Show("Vui lòng chọn một mục trong danh sách.", "Chưa chọn mục", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int idx = dgvLichSu.CurrentRow.Index;
        if (idx < 0 || idx >= _danhSach.Count)
            return;

        TuKhoaChonLai = _danhSach[idx].TuKhoa;
        DialogResult = DialogResult.OK;
        Close();
    }

    private async Task XoaTatCaAsync()
    {
        if (_danhSach.Count == 0)
        {
            MessageBox.Show("Lịch sử tra cứu đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var xacNhan = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa toàn bộ lịch sử tra cứu không?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (xacNhan != DialogResult.Yes)
            return;

        SetDangXuLy(true, "Đang xóa lịch sử...");
        var ketQua = await _service.XoaTatCaLichSuAsync();
        SetDangXuLy(false);

        if (!ketQua.ThanhCong)
        {
            lblTrangThai.Text = ketQua.ThongBao;
            MessageBox.Show(ketQua.ThongBao, "Xóa thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _danhSach.Clear();
        dgvLichSu.Rows.Clear();
        lblTrangThai.Text = ketQua.ThongBao;
        MessageBox.Show(ketQua.ThongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SetDangXuLy(bool dangXuLy, string? thongBao = null)
    {
        btnLamMoi.Enabled = !dangXuLy;
        btnTraCuuLai.Enabled = !dangXuLy;
        btnXoaTatCa.Enabled = !dangXuLy;
        Cursor = dangXuLy ? Cursors.WaitCursor : Cursors.Default;
        if (thongBao is not null)
            lblTrangThai.Text = thongBao;
    }
}