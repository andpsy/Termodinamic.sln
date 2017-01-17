using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace Termodinamic
{
    public static class CommonFunctions
    {
        public static Image ScaleImage(Image image, System.Windows.Forms.PictureBox pb)
        {
            double ratioX = (double)pb.Width / (double)image.Width;
            double ratioY = (double)pb.Height / (double)image.Height;
            double sz = (double)Math.Max(image.Width, image.Height);
            double ratio = (double)Math.Min(pb.Width, pb.Height) / sz;
            ratio = ratio > 1 ? 1 : ratio;

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
    }
}
