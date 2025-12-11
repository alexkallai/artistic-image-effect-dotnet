using System.Windows.Media.Imaging;

namespace artistic_image_effect_dotnet
{
    public static class ImageEffect
    {
        public static void DrawCircle(ref bool[,] canvas, int centerX, int centerY, int radius, int circleWidth)
        {
            int height = canvas.GetLength(0);
            int width = canvas.GetLength(1);

            // Define the bounds for iteration, limited to the canvas dimensions
            int minX = Math.Max(0, centerX - radius - circleWidth);
            int maxX = Math.Min(width - 1, centerX + radius + circleWidth);
            int minY = Math.Max(0, centerY - radius - circleWidth);
            int maxY = Math.Min(height - 1, centerY + radius + circleWidth);

            // Iterate through the limited bounds
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // Calculate the distance from the current point to the circle's center
                    double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));

                    // Check if the point is within the circle's width
                    if (distance >= radius - circleWidth && distance <= radius + circleWidth)
                    {
                        canvas[y, x] = true; // Mark the point as part of the circle
                    }
                }
            }
        }

        public static WriteableBitmap CreateWriteableBitmapFromArray(bool[,] canvas)
        {
            int height = canvas.GetLength(0);
            int width = canvas.GetLength(1);

            // Create a WriteableBitmap with the same dimensions as the array
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

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
                        if (canvas[y, x])
                        {
                            // White for true
                            pixels[y * width + x] = unchecked((int)0xFFFFFFFF); // ARGB: White
                        }
                        else
                        {
                            // Black for false
                            pixels[y * width + x] = unchecked((int)0xFF000000); // ARGB: Black
                        }
                    }
                }
            }

            // Unlock the bitmap and make it ready for rendering
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            return bitmap;
        }
    }

}
