namespace HealthClient.UI
{
    /// <summary>
    /// Bo gioi han so hieu ung dang chay cung luc (v35), de luong giao dien (main thread) khong bi nghen.
    /// Moi hieu ung huu han (go chu o bia, song quanh logo, chu dong o khau hieu...) phai xin mot "cho" truoc khi
    /// chay; het cho thi bo qua hieu ung va hien thang trang thai cuoi. Xong thi tra cho lai.
    /// Toi da 2 hieu ung cung luc (ngan sach hieu nang, quy tac 13 trong nhat ky). Chi dung tren luong giao dien nen khong can khoa.
    /// </summary>
    public static class GioiHanHoatAnh
    {
        public const int ToiDa = 2;

        private static int _dang;

        /// <summary>So hieu ung dang giu cho (de bang do hieu nang hien thi).</summary>
        public static int Dang => _dang;

        /// <summary>Xin mot cho chay. true = duoc phep chay (nho goi Tra khi xong).</summary>
        public static bool Xin()
        {
            if (_dang >= ToiDa)
                return false;

            _dang++;
            return true;
        }

        public static void Tra()
        {
            if (_dang > 0)
                _dang--;
        }
    }
}
