using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace artistic_image_effect_dotnet
{
    public static class ImageEffect
    {
        public static WriteableBitmap Render(
                                        int width,
                                        int height,
                                        double frequency,
                                        double thickness)
        {
            var field = GenerateConcentricCircles(
                width,
                height,
                frequency,
                thickness);

            return ToBlackWhiteBitmap(field);
        }

        // true = black pixel, false = white pixel
        public static bool[,] GenerateConcentricCircles(
            int width,
            int height,
            double frequency,
            double thickness)
        {
            var field = new bool[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double nx = (x / (double)width) * 2.0 - 1.0;
                    double ny = (y / (double)height) * 2.0 - 1.0;

                    double r = Math.Sqrt(nx * nx + ny * ny);
                    double value = Math.Sin(r * frequency * Math.PI * 2.0);

                    field[y, x] = Math.Abs(value) < thickness;
                }
            }

            return field;
        }

        public static WriteableBitmap ToBlackWhiteBitmap(bool[,] field)
        {
            int height = field.GetLength(0);
            int width = field.GetLength(1);

            int stride = (width + 7) / 8;
            byte[] pixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!field[y, x])
                        continue;

                    int byteIndex = y * stride + (x >> 3);
                    int bitIndex = 7 - (x & 7);

                    pixels[byteIndex] |= (byte)(1 << bitIndex);
                }
            }

            var bmp = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.BlackWhite,
                null);

            bmp.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels,
                stride,
                0);

            return bmp;
        }
    }


}
