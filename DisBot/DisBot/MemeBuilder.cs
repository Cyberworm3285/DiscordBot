using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DisBot
{
    //modified version of https://github.com/matteobortolazzo/meme-builder/blob/master/Meme/MemeBuilder.cs
    public static class MemeBuilder
    {
        public static void Build(string inputPath, string outputPath, string topText, string bottomText = null)
        {
            var bitmap = new Bitmap(inputPath);

            WriteText(bitmap, topText, StringAlignment.Near);
            if (bottomText != null)
                WriteText(bitmap, bottomText, StringAlignment.Far);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputPath);
            bitmap.Save(path, ImageFormat.Jpeg);
            bitmap.Dispose();
        }

        private static void WriteText(Image bmp, string text, StringAlignment align)
        {
            float fontSize = bmp.Width / 12;
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = align
            };

            var f = new Font("Impact", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);

            var p = new Pen(Color.Black, fontSize / 10) { LineJoin = LineJoin.Round };

            var b = new SolidBrush(Color.White);

            var r = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var gp = new GraphicsPath();

            gp.AddString(text, f.FontFamily, (int)FontStyle.Regular, fontSize, r, sf);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawPath(p, gp);
                g.FillPath(b, gp);
            }

            gp.Dispose();
            b.Dispose();
            b.Dispose();
            f.Dispose();
            sf.Dispose();
        }
    }
}