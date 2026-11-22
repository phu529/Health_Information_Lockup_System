using System.Drawing;

namespace HealthClient.UI
{
    /// <summary>
    /// Giu cua so luon nam gon trong vung lam viec cua man hinh (khong de thanh tac vu che mat),
    /// du man hinh nho hay do phong to chu cua Windows (125%, 150%...).
    /// </summary>
    public static class ManHinh
    {
        /// <param name="giuViTri">
        /// Neu co: dat cua so ngay tai vi tri nay (da kep vao trong man hinh) thay vi can giua.
        /// Dung khi chuyen qua lai giua form Dang nhap va Dang ky de hai cua so chong khit len nhau.
        /// </param>
        public static void VuaVoiManHinh(Form f, Point? giuViTri = null)
        {
            Rectangle vung = Screen.FromControl(f).WorkingArea;

            int rong = Math.Min(f.Width, vung.Width);
            int cao = Math.Min(f.Height, vung.Height);

            if (rong != f.Width || cao != f.Height)
                f.Size = new Size(rong, cao);

            int x;
            int y;

            if (giuViTri is Point g)
            {
                x = Math.Clamp(g.X, vung.X, Math.Max(vung.X, vung.Right - f.Width));
                y = Math.Clamp(g.Y, vung.Y, Math.Max(vung.Y, vung.Bottom - f.Height));
            }
            else
            {
                x = vung.X + (vung.Width - f.Width) / 2;
                y = vung.Y + (vung.Height - f.Height) / 2;
            }

            f.StartPosition = FormStartPosition.Manual;
            f.Location = new Point(x, y);
        }
    }
}
