namespace HealthClient
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            // v40: bang do CPU/RAM (an mac dinh): bam Ctrl+Shift+F9 de bat/tat khi can kiem tra hieu nang.
            HealthClient.UI.DoHieuNang.KichHoat();

            Application.SetUnhandledExceptionMode(
                UnhandledExceptionMode.CatchException);

            Application.ThreadException += (sender, args) => HienLoiChung();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => HienLoiChung();
            HealthClient.Services.AppServices.Auth = new HealthClient.Services.AuthServiceTcp();
            Application.Run(new FrmDangNhap());
        }

        private static void HienLoiChung()
        {
            MessageBox.Show(
                "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
