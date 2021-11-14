using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using RectangleD = System.Drawing.Rectangle;

namespace Utility
{
    public static class BitmapHelper
    {
        public static Bitmap CropCenterToMinimumDimension(this Bitmap image)
        {
            var min = Math.Min(image.Height, image.Width);

            System.Drawing.Point point;
            if (min == image.Width)
            {
                var offset = (image.Height - image.Width) / 2d;
                point = new System.Drawing.Point(0, (int)offset);
            }
            else
            {
                var offset = (image.Width - image.Height) / 2d;
                point = new System.Drawing.Point((int)offset, 0);
            }

            var cropRect = new RectangleD(point, new System.Drawing.Size((int)min, (int)min));
            var cropped = new Bitmap(cropRect.Width, cropRect.Height);
            using (var graphics = Graphics.FromImage(cropped))
            {
                graphics.DrawImage(image,
                    new RectangleD(0, 0, cropped.Width, cropped.Height),
                    cropRect,
                    GraphicsUnit.Pixel);
            }

            return cropped;
        }

        /// <summary>
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/a82ce006-79f9-4475-a861-136d20a86421/how-to-stream-bitmap-c?forum=csharpgeneral
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Stream ToStream(this Bitmap bitmap)
        {
            ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageEncoders().Single(x => x.FormatDescription == "PNG");
            Encoder encoder2 = Encoder.Quality;
            EncoderParameters parameters = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(encoder2, 50L);
            parameters.Param[0] = parameter;

            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, jpgEncoder, parameters);
            //bitmap.Save(@"C:\Temp\TestJPEG.jpg", jpgEncoder, parameters);

            var bytes = (stream).ToArray();
            Stream inputStream = new MemoryStream(bytes);

            return inputStream;
            //Bitmap fromDisk = new Bitmap(@"C:\Temp\TestJPEG.jpg");
            //Bitmap fromStream = new Bitmap(inputStream);
        }

        /// <summary>
        /// ResizeImage the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        public static  Stream ResizeImage(this Stream inputStream, int newWidth)
        {
            Image originalImage = System.Drawing.Image.FromStream(inputStream, true, true);
            Image resizedImage = originalImage.GetThumbnailImage(newWidth, (newWidth * originalImage.Height) / originalImage.Width, null, IntPtr.Zero);
            return resizedImage.ToStream(ImageFormat.Png);
        }

        public static Stream ToStream(this Image image, ImageFormat format)
        {
            var stream = new MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }
    }
}