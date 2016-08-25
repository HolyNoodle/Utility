using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public static class ImageHelper
    {
        public static Stream Resize(Stream input, int width, int height, bool blackBorder = true)
        {
            using (var bitmap = Bitmap.FromStream(input))
            {
                var ratio = (double)Math.Max(bitmap.Width, bitmap.Height) / (double)Math.Min(bitmap.Width, bitmap.Height);
                var newWidth = 0;
                var newHeight = 0;

                if (bitmap.Width >= bitmap.Height)
                {
                    newWidth = Math.Min(width, bitmap.Width);
                    newHeight = (int)(newWidth / ratio);
                }
                else
                {
                    newHeight = Math.Min(height, bitmap.Height);
                    newWidth = (int)(newHeight / ratio);
                }
                if (blackBorder)
                {
                    using (var newImage = new Bitmap(width, height))
                    {
                        using (var graphic = Graphics.FromImage(newImage))
                        {
                            graphic.FillRectangle(Brushes.Black, 0, 0, width, height);
                            var positionX = 0;
                            var positionY = 0;
                            if (newWidth < width)
                            {
                                positionX = (width - newWidth) / 2;
                            }
                            if (newHeight < height)
                            {
                                positionY = (height - newHeight) / 2;
                            }

                            graphic.DrawImage(bitmap, positionX, positionY, newWidth, newHeight);
                            graphic.Save();
                        }

                        var memory = new MemoryStream();
                        newImage.Save(memory, ImageFormat.Png);

                        return memory;
                    }
                }
                else
                {
                    using (var newImage = new Bitmap(bitmap, newWidth, newHeight))
                    {
                        var memory = new MemoryStream();
                        newImage.Save(memory, ImageFormat.Png);

                        return memory;
                    }
                }
            }
        }

        public static Image CorrectImageOrientation(Image image)
        {
            if (Array.IndexOf(image.PropertyIdList, 274) > -1)
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                image.RemovePropertyItem(274);
            }
            return image;
        }
    }
}
