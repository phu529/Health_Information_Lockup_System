using System.Runtime.InteropServices;
using System.Text;

namespace HealthClient.Services
{
    /// <summary>
    /// Luu tren may nguoi dung ten dang nhap va (v24) mat khau de tick "Nho mat khau".
    /// Mat khau KHONG bao gio luu dang chu thuong: ma hoa bang Windows DPAPI (pham vi nguoi dung hien tai),
    /// nen chep file sang may khac hoac tai khoan Windows khac thi khong doc duoc.
    /// </summary>
    public static class CauHinhCucBo
    {
        private static string DuongDan => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HealthClient", "tai-khoan.txt");

        public static string? DocTenDaNho()
        {
            try
            {
                return File.Exists(DuongDan) ? File.ReadAllText(DuongDan).Trim() : null;
            }
            catch (Exception)
            {
                // Khong doc duoc thi coi nhu chua nho, khong anh huong dang nhap.
                return null;
            }
        }

        public static void LuuTenDaNho(string? ten)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ten))
                {
                    if (File.Exists(DuongDan))
                        File.Delete(DuongDan);

                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(DuongDan)!);
                File.WriteAllText(DuongDan, ten.Trim());
            }
            catch (Exception)
            {
                // Khong ghi duoc thi bo qua, khong lam hong luong dang nhap.
            }
        }

        // ----- v24: mat khau da nho, ma hoa bang DPAPI -----

        private static string DuongDanMatKhau => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HealthClient", "mat-khau.bin");

        public static string? DocMatKhauDaNho()
        {
            try
            {
                if (!File.Exists(DuongDanMatKhau))
                    return null;

                byte[]? giai = Boc(File.ReadAllBytes(DuongDanMatKhau), maHoa: false);
                return giai == null ? null : Encoding.UTF8.GetString(giai);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void LuuMatKhauDaNho(string? matKhau)
        {
            try
            {
                if (string.IsNullOrEmpty(matKhau))
                {
                    if (File.Exists(DuongDanMatKhau))
                        File.Delete(DuongDanMatKhau);

                    return;
                }

                byte[]? ma = Boc(Encoding.UTF8.GetBytes(matKhau), maHoa: true);

                // Khong ma hoa duoc thi KHONG luu (khong bao gio ghi chu thuong).
                if (ma == null)
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(DuongDanMatKhau)!);
                File.WriteAllBytes(DuongDanMatKhau, ma);
            }
            catch (Exception)
            {
                // Khong ghi duoc thi bo qua, khong lam hong luong dang nhap.
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DataBlob
        {
            public int cbData;
            public IntPtr pbData;
        }

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptProtectData(
            ref DataBlob dataIn, string? moTa, IntPtr entropy, IntPtr danhChoSau,
            IntPtr prompt, int co, out DataBlob dataOut);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptUnprotectData(
            ref DataBlob dataIn, IntPtr moTa, IntPtr entropy, IntPtr danhChoSau,
            IntPtr prompt, int co, out DataBlob dataOut);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr h);

        private static byte[]? Boc(byte[] vao, bool maHoa)
        {
            var blobVao = new DataBlob { cbData = vao.Length, pbData = Marshal.AllocHGlobal(vao.Length) };
            DataBlob blobRa = default;

            try
            {
                Marshal.Copy(vao, 0, blobVao.pbData, vao.Length);

                bool ok = maHoa
                    ? CryptProtectData(ref blobVao, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, out blobRa)
                    : CryptUnprotectData(ref blobVao, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, out blobRa);

                if (!ok)
                    return null;

                var ra = new byte[blobRa.cbData];
                Marshal.Copy(blobRa.pbData, ra, 0, blobRa.cbData);
                return ra;
            }
            finally
            {
                Marshal.FreeHGlobal(blobVao.pbData);

                if (blobRa.pbData != IntPtr.Zero)
                    LocalFree(blobRa.pbData);
            }
        }
    }
}
