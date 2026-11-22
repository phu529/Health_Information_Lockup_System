using System.Drawing;

namespace HealthClient.UI
{
    /// <summary>
    /// Bang mau chu "len mau" khi re chuot (v32), lay theo hieu ung chu cua trang coolors.co ("Make something colorful!"):
    /// xanh troi -> chàm -> tim -> hong -> do -> cam -> vang -> xanh la, chay doc theo vi tri chu trong tu.
    /// Nen toi (nut navy, nut co hinh nen xanh dam): pha 18% trang cho sang. Nen sang (nut vien trang, nut aqua):
    /// pha 16% den de chu van doc duoc.
    /// </summary>
    public static class PhoMau
    {
        private static readonly float[] Moc = { 0f, 0.16f, 0.32f, 0.48f, 0.64f, 0.78f, 0.90f, 1f };

        private static readonly Color[] Mau =
        {
            Color.FromArgb(14, 165, 233),
            Color.FromArgb(99, 102, 241),
            Color.FromArgb(168, 85, 247),
            Color.FromArgb(236, 72, 153),
            Color.FromArgb(239, 68, 68),
            Color.FromArgb(249, 115, 22),
            Color.FromArgb(234, 179, 8),
            Color.FromArgb(34, 197, 94)
        };

        /// <param name="t">Vi tri trong tu, 0..1 (chu dau = 0, chu cuoi = 1).</param>
        /// <param name="nenToi">true neu chu nam tren nen toi.</param>
        public static Color Lay(float t, bool nenToi)
        {
            t = Math.Clamp(t, 0f, 1f);

            int k = 0;

            while (k < Moc.Length - 2 && t > Moc[k + 1])
                k++;

            float u = (t - Moc[k]) / (Moc[k + 1] - Moc[k]);
            Color a = Mau[k];
            Color b = Mau[k + 1];

            var c = Color.FromArgb(
                (int)Math.Round(a.R + (b.R - a.R) * u),
                (int)Math.Round(a.G + (b.G - a.G) * u),
                (int)Math.Round(a.B + (b.B - a.B) * u));

            return nenToi ? Pha(c, 255, 0.18f) : Pha(c, 0, 0.16f);
        }

        private static Color Pha(Color c, int den, float tyLe)
        {
            return Color.FromArgb(
                (int)Math.Round(c.R + (den - c.R) * tyLe),
                (int)Math.Round(c.G + (den - c.G) * tyLe),
                (int)Math.Round(c.B + (den - c.B) * tyLe));
        }
    }
}
