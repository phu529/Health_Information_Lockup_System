using System.Drawing;
using System.Drawing.Drawing2D;

namespace HealthClient.UIToiGian
{
    internal enum LucideIconKind
    {
        None,
        BookOpen,
        Pill,
        Shield,
        ShieldCheck,
        Award,
        ListChecks,
        Search,
        Eye,
        EyeOff,
        Info,
        User,
        Lock,
        Mail,
        Close,
        ArrowRight,
        ArrowLeft,
        HeartPulse,
        Database,
        Users,
        FileText,
        Check,
        TriangleAlert,
        Heart,
        CircleCheck,
        Send,
        Clock,
        Sparkles
    }

    // Ve bieu tuong Lucide chuan 24x24 (net phang, dau tron muot, fill=none).
    internal static class LucideIcons
    {
        public static void Draw(Graphics g, LucideIconKind kind, RectangleF bounds, Color color, float strokeRatio = 2.0f)
        {
            if (kind == LucideIconKind.None || bounds.Width < 4f || bounds.Height < 4f) return;

            float side = Math.Min(bounds.Width, bounds.Height);
            float ox = bounds.X + (bounds.Width - side) / 2f;
            float oy = bounds.Y + (bounds.Height - side) / 2f;
            float u = side / 24f;
            float stroke = Math.Max(1.45f, u * strokeRatio);

            SmoothingMode oldSmooth = g.SmoothingMode;
            PixelOffsetMode oldPixel = g.PixelOffsetMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var pen = new Pen(color, stroke)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            using var dotBrush = new SolidBrush(color);

            PointF P(float x, float y) => new(ox + x * u, oy + y * u);
            RectangleF R(float x, float y, float w, float h) => new(ox + x * u, oy + y * u, w * u, h * u);

            switch (kind)
            {
                case LucideIconKind.BookOpen:
                    g.DrawLine(pen, P(12, 7), P(12, 21));
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(12, 7), P(12, 4.5f), P(10, 3), P(7.5f, 3));
                        p.AddLine(P(7.5f, 3), P(3, 3));
                        p.AddLine(P(3, 3), P(2, 4));
                        p.AddLine(P(2, 4), P(2, 17));
                        p.AddLine(P(2, 17), P(3, 18));
                        p.AddLine(P(3, 18), P(8.5f, 18));
                        p.AddBezier(P(8.5f, 18), P(10.5f, 18), P(12, 19.2f), P(12, 21));
                        g.DrawPath(pen, p);
                    }
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(12, 7), P(12, 4.5f), P(14, 3), P(16.5f, 3));
                        p.AddLine(P(16.5f, 3), P(21, 3));
                        p.AddLine(P(21, 3), P(22, 4));
                        p.AddLine(P(22, 4), P(22, 17));
                        p.AddLine(P(22, 17), P(21, 18));
                        p.AddLine(P(21, 18), P(15.5f, 18));
                        p.AddBezier(P(15.5f, 18), P(13.5f, 18), P(12, 19.2f), P(12, 21));
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, P(5.2f, 8.2f), P(9.0f, 8.2f));
                    g.DrawLine(pen, P(5.2f, 12.2f), P(9.0f, 12.2f));
                    g.DrawLine(pen, P(15.0f, 8.2f), P(18.8f, 8.2f));
                    g.DrawLine(pen, P(15.0f, 12.2f), P(18.8f, 12.2f));
                    break;

                case LucideIconKind.Pill:
                    {
                        GraphicsState st = g.Save();
                        g.TranslateTransform(ox + 12 * u, oy + 12 * u);
                        g.RotateTransform(-45f);
                        var pillRect = new RectangleF(-10.0f * u, -4.8f * u, 20.0f * u, 9.6f * u);
                        using (var p = AppTheme.DuongBoGoc(pillRect, 4.8f * u))
                            g.DrawPath(pen, p);
                        g.DrawLine(pen, 0f, -4.8f * u, 0f, 4.8f * u);
                        g.Restore(st);
                    }
                    break;

                case LucideIconKind.Shield:
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(P(12, 2.3f), P(20, 5.2f));
                        p.AddBezier(P(20, 5.2f), P(20, 12.2f), P(17.5f, 18.2f), P(12, 21.8f));
                        p.AddBezier(P(12, 21.8f), P(6.5f, 18.2f), P(4, 12.2f), P(4, 5.2f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, P(12, 8.5f), P(12, 15.5f));
                    g.DrawLine(pen, P(8.5f, 12f), P(15.5f, 12f));
                    break;

                case LucideIconKind.ShieldCheck:
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(P(12, 2.3f), P(20, 5.2f));
                        p.AddBezier(P(20, 5.2f), P(20, 12.2f), P(17.5f, 18.2f), P(12, 21.8f));
                        p.AddBezier(P(12, 21.8f), P(6.5f, 18.2f), P(4, 12.2f), P(4, 5.2f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLines(pen, new[] { P(8.8f, 12.2f), P(11.1f, 14.5f), P(15.6f, 9.6f) });
                    break;

                case LucideIconKind.Award:
                    // Huy hieu uy tin (phu hop "Tin cay")
                    g.DrawEllipse(pen, R(6, 2.5f, 12, 12));
                    g.DrawLines(pen, new[] { P(9.4f, 8.6f), P(11.2f, 10.4f), P(14.8f, 6.8f) });
                    g.DrawLines(pen, new[] { P(8.4f, 13.6f), P(7.2f, 21.5f), P(12f, 18.8f), P(16.8f, 21.5f), P(15.6f, 13.6f) });
                    break;

                case LucideIconKind.ListChecks:
                    g.DrawLines(pen, new[] { P(3, 6.5f), P(5, 8.5f), P(8.5f, 4.8f) });
                    g.DrawLine(pen, P(12, 7), P(21, 7));
                    g.DrawLines(pen, new[] { P(3, 12.5f), P(5, 14.5f), P(8.5f, 10.8f) });
                    g.DrawLine(pen, P(12, 13), P(21, 13));
                    g.DrawLines(pen, new[] { P(3, 18.5f), P(5, 20.5f), P(8.5f, 16.8f) });
                    g.DrawLine(pen, P(12, 19), P(21, 19));
                    break;

                case LucideIconKind.Search:
                    g.DrawEllipse(pen, R(3, 3, 15.5f, 15.5f));
                    g.DrawLine(pen, P(16.4f, 16.4f), P(21, 21));
                    break;

                case LucideIconKind.Eye:
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(2, 12), P(5, 6), P(9, 5), P(12, 5));
                        p.AddBezier(P(12, 5), P(15, 5), P(19, 6), P(22, 12));
                        p.AddBezier(P(22, 12), P(19, 18), P(15, 19), P(12, 19));
                        p.AddBezier(P(12, 19), P(9, 19), P(5, 18), P(2, 12));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawEllipse(pen, R(9, 9, 6, 6));
                    break;

                case LucideIconKind.EyeOff:
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(10.7f, 5.1f), P(14.6f, 4.7f), P(18.8f, 6.6f), P(22, 12));
                        p.AddBezier(P(22, 12), P(21.1f, 13.5f), P(20.1f, 14.8f), P(18.9f, 15.8f));
                        g.DrawPath(pen, p);
                    }
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(6.6f, 6.7f), P(4.7f, 8f), P(3.2f, 9.7f), P(2, 12));
                        p.AddBezier(P(2, 12), P(5.2f, 17.5f), P(9.4f, 19.2f), P(13.8f, 18.8f));
                        p.AddBezier(P(13.8f, 18.8f), P(14.9f, 18.6f), P(15.9f, 18.2f), P(16.9f, 17.6f));
                        g.DrawPath(pen, p);
                    }
                    g.DrawArc(pen, R(9, 9, 6, 6), 135f, 175f);
                    g.DrawLine(pen, P(3, 3), P(21, 21));
                    break;

                case LucideIconKind.Info:
                    g.DrawEllipse(pen, R(2, 2, 20, 20));
                    g.DrawLine(pen, P(12, 16), P(12, 11.5f));
                    {
                        float rDot = Math.Max(1.2f, stroke * 0.65f);
                        g.FillEllipse(dotBrush, ox + 12 * u - rDot, oy + 7.8f * u - rDot, rDot * 2f, rDot * 2f);
                    }
                    break;

                case LucideIconKind.User:
                    g.DrawEllipse(pen, R(8, 3.5f, 8, 8));
                    g.DrawArc(pen, R(4.5f, 14f, 15f, 12f), 185f, 170f);
                    break;

                case LucideIconKind.Lock:
                    using (var p = AppTheme.DuongBoGoc(R(3.5f, 11, 17, 10.5f), 2.2f * u))
                        g.DrawPath(pen, p);
                    g.DrawArc(pen, R(7.5f, 3f, 9, 9), 180f, 180f);
                    g.DrawLine(pen, P(7.5f, 7.5f), P(7.5f, 11));
                    g.DrawLine(pen, P(16.5f, 7.5f), P(16.5f, 11));
                    break;

                case LucideIconKind.Mail:
                    using (var p = AppTheme.DuongBoGoc(R(2.5f, 4.5f, 19, 15), 2.2f * u))
                        g.DrawPath(pen, p);
                    g.DrawLines(pen, new[] { P(3.2f, 7.2f), P(12, 13f), P(20.8f, 7.2f) });
                    break;

                case LucideIconKind.Close:
                    g.DrawLine(pen, P(17, 7), P(7, 17));
                    g.DrawLine(pen, P(7, 7), P(17, 17));
                    break;

                case LucideIconKind.ArrowRight:
                    g.DrawLine(pen, P(5, 12), P(19, 12));
                    g.DrawLines(pen, new[] { P(12.5f, 5.5f), P(19, 12), P(12.5f, 18.5f) });
                    break;

                case LucideIconKind.ArrowLeft:
                    g.DrawLine(pen, P(19, 12), P(5, 12));
                    g.DrawLines(pen, new[] { P(11.5f, 5.5f), P(5, 12), P(11.5f, 18.5f) });
                    break;

                case LucideIconKind.HeartPulse:
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(12, 20.5f), P(4.5f, 15.5f), P(2.2f, 10.5f), P(3.2f, 7.2f));
                        p.AddBezier(P(3.2f, 7.2f), P(4.5f, 3.8f), P(9.2f, 3.5f), P(12, 6.8f));
                        p.AddBezier(P(12, 6.8f), P(14.8f, 3.5f), P(19.5f, 3.8f), P(20.8f, 7.2f));
                        p.AddBezier(P(20.8f, 7.2f), P(21.8f, 10.5f), P(19.5f, 15.5f), P(12, 20.5f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLines(pen, new[]
                    {
                        P(5.5f, 12), P(8.5f, 12), P(10.5f, 8.8f),
                        P(13f, 15.2f), P(15f, 11), P(16.5f, 12), P(18.5f, 12)
                    });
                    break;

                case LucideIconKind.Heart:
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(12, 20.5f), P(4.5f, 15.5f), P(2.2f, 10.5f), P(3.2f, 7.2f));
                        p.AddBezier(P(3.2f, 7.2f), P(4.5f, 3.8f), P(9.2f, 3.5f), P(12, 6.8f));
                        p.AddBezier(P(12, 6.8f), P(14.8f, 3.5f), P(19.5f, 3.8f), P(20.8f, 7.2f));
                        p.AddBezier(P(20.8f, 7.2f), P(21.8f, 10.5f), P(19.5f, 15.5f), P(12, 20.5f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    break;

                case LucideIconKind.Database:
                    g.DrawEllipse(pen, R(3.5f, 3f, 17, 6));
                    g.DrawLine(pen, P(3.5f, 6), P(3.5f, 18));
                    g.DrawLine(pen, P(20.5f, 6), P(20.5f, 18));
                    g.DrawArc(pen, R(3.5f, 9f, 17, 6), 0f, 180f);
                    g.DrawArc(pen, R(3.5f, 15f, 17, 6), 0f, 180f);
                    break;

                case LucideIconKind.Users:
                    g.DrawEllipse(pen, R(5.5f, 4, 7, 7));
                    g.DrawArc(pen, R(2.5f, 14f, 13f, 11f), 185f, 170f);
                    g.DrawArc(pen, R(13.5f, 4.5f, 6, 6), -80f, 160f);
                    g.DrawArc(pen, R(14.5f, 14.2f, 7.5f, 10f), 270f, 85f);
                    break;

                case LucideIconKind.FileText:
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(P(14, 2.5f), P(6, 2.5f));
                        p.AddLine(P(6, 2.5f), P(4.5f, 4));
                        p.AddLine(P(4.5f, 4), P(4.5f, 20));
                        p.AddLine(P(4.5f, 20), P(6, 21.5f));
                        p.AddLine(P(6, 21.5f), P(18, 21.5f));
                        p.AddLine(P(18, 21.5f), P(19.5f, 20));
                        p.AddLine(P(19.5f, 20), P(19.5f, 8));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLines(pen, new[] { P(14, 2.5f), P(14, 8), P(19.5f, 8) });
                    g.DrawLine(pen, P(8.5f, 12.5f), P(15.5f, 12.5f));
                    g.DrawLine(pen, P(8.5f, 16.5f), P(15.5f, 16.5f));
                    break;

                case LucideIconKind.Check:
                    g.DrawLines(pen, new[] { P(4.5f, 12.5f), P(9.5f, 17.5f), P(19.5f, 7f) });
                    break;

                case LucideIconKind.CircleCheck:
                    g.DrawEllipse(pen, R(2.5f, 2.5f, 19, 19));
                    g.DrawLines(pen, new[] { P(8.5f, 12.3f), P(11.2f, 15f), P(16.2f, 9.5f) });
                    break;

                case LucideIconKind.TriangleAlert:
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(10.3f, 4.0f), P(11.1f, 2.6f), P(12.9f, 2.6f), P(13.7f, 4.0f));
                        p.AddLine(P(13.7f, 4.0f), P(21.2f, 17.6f));
                        p.AddBezier(P(21.2f, 17.6f), P(22.0f, 19.0f), P(21.0f, 20.6f), P(19.4f, 20.6f));
                        p.AddLine(P(19.4f, 20.6f), P(4.6f, 20.6f));
                        p.AddBezier(P(4.6f, 20.6f), P(3.0f, 20.6f), P(2.0f, 19.0f), P(2.8f, 17.6f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, P(12, 9.2f), P(12, 13.8f));
                    {
                        float rDot = Math.Max(1.25f, stroke * 0.68f);
                        g.FillEllipse(dotBrush, ox + 12 * u - rDot, oy + 17.2f * u - rDot, rDot * 2f, rDot * 2f);
                    }
                    break;

                case LucideIconKind.Send:
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(P(21.5f, 2.5f), P(2.5f, 10.5f));
                        p.AddLine(P(2.5f, 10.5f), P(10.2f, 13.8f));
                        p.AddLine(P(10.2f, 13.8f), P(13.5f, 21.5f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, P(21.5f, 2.5f), P(10.2f, 13.8f));
                    break;

                case LucideIconKind.Clock:
                    g.DrawEllipse(pen, R(2.5f, 2.5f, 19, 19));
                    g.DrawLines(pen, new[] { P(12, 7), P(12, 12), P(15.6f, 14) });
                    break;

                case LucideIconKind.Sparkles:
                    using (var p = new GraphicsPath())
                    {
                        p.AddBezier(P(11, 2.5f), P(11.4f, 7.6f), P(14.4f, 10.6f), P(19.5f, 11));
                        p.AddBezier(P(19.5f, 11), P(14.4f, 11.4f), P(11.4f, 14.4f), P(11, 19.5f));
                        p.AddBezier(P(11, 19.5f), P(10.6f, 14.4f), P(7.6f, 11.4f), P(2.5f, 11));
                        p.AddBezier(P(2.5f, 11), P(7.6f, 10.6f), P(10.6f, 7.6f), P(11, 2.5f));
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, P(19, 16), P(19, 21));
                    g.DrawLine(pen, P(16.5f, 18.5f), P(21.5f, 18.5f));
                    break;
            }

            g.PixelOffsetMode = oldPixel;
            g.SmoothingMode = oldSmooth;
        }
    }
}
