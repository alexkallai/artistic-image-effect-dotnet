using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace artistic_image_effect_dotnet
{
    public static class ImageEffect
    {
        public static void DrawCircle(
            ref bool[,] canvas,
            int centerX,
            int centerY,
            int radius,
            int circleWidth,
            bool filled)
        {
            int height = canvas.GetLength(0);
            int width = canvas.GetLength(1);

            int minX = Math.Max(0, centerX - radius - circleWidth);
            int maxX = Math.Min(width - 1, centerX + radius + circleWidth);
            int minY = Math.Max(0, centerY - radius - circleWidth);
            int maxY = Math.Min(height - 1, centerY + radius + circleWidth);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    double distance = Math.Sqrt((x - centerX) * (x - centerX) +
                                                (y - centerY) * (y - centerY));

                    if (filled)
                    {
                        if (distance <= radius)
                        {
                            canvas[y, x] = true;
                        }
                    }
                    else
                    {
                        if (distance >= radius - circleWidth &&
                            distance <= radius + circleWidth)
                        {
                            canvas[y, x] = true;
                        }
                    }
                }
            }
        }

        public static uint ConvertColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
        }

        public static WriteableBitmap CreateWriteableBitmapFromArray(
            bool[,] canvas,
            uint drawColor = 0xFFFFFFFF,  // Default: White
            uint backgroundColor = 0xFF000000) // Default: Black
        {
            int height = canvas.GetLength(0);
            int width = canvas.GetLength(1);

            // Create a WriteableBitmap with the same dimensions as the array
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96,
                System.Windows.Media.PixelFormats.Bgra32, null);

            // Lock the bitmap for writing
            bitmap.Lock();

            unsafe
            {
                // Get a pointer to the back buffer
                int* pixels = (int*)bitmap.BackBuffer;

                // Iterate through the array and set pixels
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Set pixel colour based on the array value
                        pixels[y * width + x] = canvas[y, x] ?
                            unchecked((int)drawColor) :
                            unchecked((int)backgroundColor);
                    }
                }
            }

            // Unlock the bitmap and make it ready for rendering
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            return bitmap;
        }

        public static bool[,] DrawFilledCirclesWidthSizes(bool[,] baseCanvas, WriteableBitmap imageBitmap)
        {
            int height = baseCanvas.GetLength(0);
            int width = baseCanvas.GetLength(1);
            var drawCanvas = new bool[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (baseCanvas[y, x] == true)
                    {
                        int radius = Convert.ToInt32(5 * GetAverageBrightness(ref imageBitmap, x, y, 20));
                        DrawCircle(ref drawCanvas, x, y, radius, 1, true);
                    }
                }
            }

            return drawCanvas;
        }

        public static double GetAverageBrightness(ref WriteableBitmap canvas, int x, int y, int radius)
        {
            canvas.Lock();

            int width = canvas.PixelWidth;
            int height = canvas.PixelHeight;

            unsafe
            {
                int* pixels = (int*)canvas.BackBuffer;

                int minX = Math.Max(0, x - radius);
                int maxX = Math.Min(width - 1, x + radius);
                int minY = Math.Max(0, y - radius);
                int maxY = Math.Min(height - 1, y + radius);

                long totalBrightness = 0;
                int pixelCount = 0;

                for (int j = minY; j <= maxY; j++)
                {
                    for (int i = minX; i <= maxX; i++)
                    {
                        int pixel = pixels[j * width + i];

                        int r = (pixel >> 16) & 0xFF;
                        int g = (pixel >> 8) & 0xFF;
                        int b = pixel & 0xFF;

                        int brightness = (r + g + b) / 3;

                        totalBrightness += brightness;
                        pixelCount++;
                    }
                }

                canvas.Unlock();

                if (pixelCount == 0)
                    return 0.0;

                double avg = (double)totalBrightness / pixelCount;

                // convert 0–255 to 0–1
                return avg / 255.0;
            }
        }



    }

}
