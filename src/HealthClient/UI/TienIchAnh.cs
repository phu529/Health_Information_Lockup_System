global using static HealthClient.UI.TienIchAnh;

using System.Drawing;
using System.Drawing.Imaging;

namespace HealthClient.UI
{
    /// <summary>
    /// Ham dung chung cho ve anh (v35b). Bo sung ham DoTrong ma LopChuyenTrang (ban v37) goi nhung khong con dinh nghia ban dau
    /// (loi CS0103 "The name 'DoTrong' does not exist"). Nho dong "global using static" o dau tep nay, moi ten
    /// DoTrong(...) trong du an tu tim thay ham nay; neu lop nao tu dinh nghia DoTrong rieng thi ham cua lop do duoc uu tien.
    /// Noi dung ham giong het ban goc trong LopChuyenTrang: tao ImageAttributes co do trong suot (alpha 0..1) de ve anh mo dan.
    /// Moi lan goi tra ve mot ImageAttributes MOI, noi goi phai Dispose (using), nhu cu.
    /// </summary>
    public static class TienIchAnh
    {
        public static ImageAttributes DoTrong(float alpha)
        {
            var m = new ColorMatrix { Matrix33 = Math.Clamp(alpha, 0f, 1f) };
            var ia = new ImageAttributes();
            ia.SetColorMatrix(m, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            return ia;
        }
    }
}
