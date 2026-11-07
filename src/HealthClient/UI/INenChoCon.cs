using System.Drawing;

namespace HealthClient.UI
{
    /// <summary>
    /// v42: control cha co nen KHONG phang (gradient) cho phep nut con ve lai dung vung nen phia sau no,
    /// de goc bo tron cua nut khong lo mot khoi mau phang. Chi la giao dien, khong cham toi logic.
    /// </summary>
    public interface INenChoCon
    {
        /// <param name="vung">Khung cua nut con theo toa do cua control cha.</param>
        void VeNenChoCon(Graphics g, Rectangle vung);
    }
}
