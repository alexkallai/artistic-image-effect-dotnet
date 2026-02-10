using artistic_image_effect_dotnet;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dotnet8ThreeColumnViewer
{
    public partial class MainWindow : Window
    {
        // Keep the images for each column accessible programmatically
        private readonly BitmapSource?[] _columnBitmaps = new BitmapSource?[3];
        private bool[,] canvas;
        private int loadedImageWidth;
        private int loadedImageHeight;
        WriteableBitmap precalculatedBitmap;

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            // Add KeyDown handler for Escape key
            this.KeyDown += MainWindow_KeyDown;

        }
        private void Image_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image clickedImage && clickedImage.Source != null)
            {
                OverlayImage.Source = clickedImage.Source;
                ImageOverlay.Visibility = Visibility.Visible;

                // Set focus to enable keyboard input
                ImageOverlay.Focus();
            }
        }

        private void ImageOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            CloseOverlay();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && ImageOverlay.Visibility == Visibility.Visible)
            {
                CloseOverlay();
                e.Handled = true;
            }
        }

        private void CloseOverlay()
        {
            ImageOverlay.Visibility = Visibility.Collapsed;
            OverlayImage.Source = null;
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
                Title = "Open image file"
            };

            if (dlg.ShowDialog(this) == true)
            {
                LoadFileToColumn(dlg.FileName, 0, (float)TargetMegapixels.Value!);
            }
        }

        private void LoadFileToColumn(string path, int columnIndex, float targetMegapixels)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show(this, "File does not exist:", path, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Load with OnLoad so file is not locked and we can save later
                var bi = new BitmapImage();

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(path);
                bi.EndInit();
                bi.Freeze();

                // Calculate current megapixels
                int originalWidth = bi.PixelWidth;
                int originalHeight = bi.PixelHeight;
                double currentMegapixels = (originalWidth * originalHeight) / 1_000_000.0;

                BitmapSource finalBitmap;

                if (targetMegapixels > 0 && Math.Abs(currentMegapixels - targetMegapixels) > 0.01)
                {
                    // Calculate scale factor to reach target megapixels
                    double scaleFactor = Math.Sqrt(targetMegapixels / currentMegapixels);

                    int newWidth = (int)(originalWidth * scaleFactor);
                    int newHeight = (int)(originalHeight * scaleFactor);

                    // Resize using TransformedBitmap
                    var transform = new ScaleTransform(scaleFactor, scaleFactor);
                    var transformedBitmap = new TransformedBitmap(bi, transform);

                    // Convert to WriteableBitmap to ensure proper pixel dimensions
                    var resizedBitmap = new WriteableBitmap(
                        new FormatConvertedBitmap(transformedBitmap, PixelFormats.Pbgra32, null, 0));
                    resizedBitmap.Freeze();

                    finalBitmap = resizedBitmap;
                    loadedImageWidth = newWidth;
                    loadedImageHeight = newHeight;
                }
                else
                {
                    finalBitmap = bi;
                    loadedImageWidth = originalWidth;
                    loadedImageHeight = originalHeight;
                }

                _columnBitmaps[columnIndex] = finalBitmap;
                SetImageSourceForColumn(columnIndex, finalBitmap);

                CallSelectedPipeline();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to load image: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CallSelectedPipeline()
        {
            Dictionary<string, Action> tabPairing = new Dictionary<string, Action>()
            {
                { "Concentric circles", ConcentricCirclesPipeline},
                { "Phyllotaxis spiral", PhyllotaxisSpiralPipeline }
            };
            var selected = GlobalControlTabs.SelectedItem as TabItem;
            var selectedString = selected?.Header.ToString();
            System.Diagnostics.Debug.WriteLine($"Selected tab is: {selectedString}");
            if (selected != null && selectedString != null && tabPairing.ContainsKey(selectedString))
            {
                tabPairing[selectedString]();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not call the pipeline!");
            }
        }

        // At this point we assume the initial image is already loaded
        private void ConcentricCirclesPipeline()
        {
            canvas = new bool[loadedImageHeight, loadedImageWidth];

            // Do the drawing stuff on the canvas

            for (int i = 0; i < Convert.ToInt32(ConcIntNum.Value); i++)
            {
                ImageEffect.DrawCircle(ref canvas,
                    Convert.ToInt32(ConcIntX.Value),
                    Convert.ToInt32(ConcIntY.Value),
                    i * Convert.ToInt32(ConcIntInc.Value),
                    Convert.ToInt32(ConcIntLineWidth.Value),
                    false);
            }
            var precalculatedBitmap = new WriteableBitmap(_columnBitmaps[0]);
            var drawnCanvas = ImageEffect.DrawFilledCirclesWidthSizes(canvas, precalculatedBitmap, Convert.ToInt32(ConcIntCircleSize.Value), Convert.ToInt32(ConcIntBrightnessAverageDistance.Value));


            // Set the images
            if (GlobalColor1.SelectedColor != null && GlobalColor2.SelectedColor != null)
            {

                // Set the middle image
                var bitmapCanvas = ImageEffect.CreateWriteableBitmapFromArray(canvas);
                _columnBitmaps[1] = bitmapCanvas;
                SetImageSourceForColumn(1, bitmapCanvas);

                // Set the right output image
                var drawnBitmapCanvas = ImageEffect.CreateWriteableBitmapFromArray(drawnCanvas,
                    ImageEffect.ConvertColorToUInt(GlobalColor1.SelectedColor.Value),
                    ImageEffect.ConvertColorToUInt(GlobalColor2.SelectedColor.Value));
                _columnBitmaps[2] = drawnBitmapCanvas;
                SetImageSourceForColumn(2, drawnBitmapCanvas);
            }

        }

        private void PhyllotaxisSpiralPipeline()
        {
            canvas = new bool[loadedImageHeight, loadedImageWidth];

            int count = Convert.ToInt32(PhyIntNum.Value);
            int centerX = Convert.ToInt32(PhyIntX.Value);
            int centerY = Convert.ToInt32(PhyIntY.Value);
            int scale = Convert.ToInt32(PhyIntInc.Value);
            int pointRadius = Convert.ToInt32(PhyIntLineWidth.Value);
            int maxVariableSize = Convert.ToInt32(PhyIntCircleSize.Value);
            int brightnessDistance = Convert.ToInt32(PhyIntBrightnessAverageDistance.Value);

            double goldenAngle = Math.PI * (3.0 - Math.Sqrt(5.0)); // ~137.5°

            for (int i = 0; i < count; i++)
            {
                double radius = Math.Sqrt(i) * scale;
                double angle = i * goldenAngle;

                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));

                ImageEffect.DrawCircle(
                    ref canvas,
                    x,
                    y,
                    pointRadius,
                    pointRadius,
                    true);
            }

            var precalculatedBitmap = new WriteableBitmap(_columnBitmaps[0]);
            var drawnCanvas = ImageEffect.DrawFilledCirclesWidthSizes(
                canvas,
                precalculatedBitmap,
                maxVariableSize,
                brightnessDistance);

            if (GlobalColor1.SelectedColor != null && GlobalColor2.SelectedColor != null)
            {
                var bitmapCanvas = ImageEffect.CreateWriteableBitmapFromArray(canvas);
                _columnBitmaps[1] = bitmapCanvas;
                SetImageSourceForColumn(1, bitmapCanvas);

                var drawnBitmapCanvas = ImageEffect.CreateWriteableBitmapFromArray(
                    drawnCanvas,
                    ImageEffect.ConvertColorToUInt(GlobalColor1.SelectedColor.Value),
                    ImageEffect.ConvertColorToUInt(GlobalColor2.SelectedColor.Value));

                _columnBitmaps[2] = drawnBitmapCanvas;
                SetImageSourceForColumn(2, drawnBitmapCanvas);
            }
        }



        private void SetImageSourceForColumn(int columnIndex, BitmapSource? bmp)
        {
            switch (columnIndex)
            {
                case 0: Image0.Source = bmp; break;
                case 1: Image1.Source = bmp; break;
                case 2: Image2.Source = bmp; break;
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is string tagStr)
            {
                if (!int.TryParse(tagStr, out var idx)) return;
            }

            if (sender is FrameworkElement fe2 && fe2.Tag is int idxInt)
            {
                SaveBitmapAtColumn(idxInt);
                return;
            }

            // If Tag was set as string in XAML, try parsing from the control's Tag property.
            if (sender is FrameworkElement fe3 && fe3.Tag != null)
            {
                if (int.TryParse(fe3.Tag.ToString(), out var idx))
                {
                    SaveBitmapAtColumn(idx);
                    return;
                }
            }

            // fallback — shouldn't normally happen
            MessageBox.Show(this, "Unknown column index for save.", "Save", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SaveBitmapAtColumn(int columnIndex)
        {
            var bmp = _columnBitmaps[columnIndex];
            if (bmp == null)
            {
                MessageBox.Show(this, "No image in that column to save.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                DefaultExt = "png",
                FileName = "image.png"
            };

            if (sfd.ShowDialog(this) != true) return;

            try
            {
                using var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to save image: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Accept drops and treat the first file as Open -> leftmost column
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            // Use the first file. Behavior matches Open: put into leftmost column
            LoadFileToColumn(files[0], 0, (float)TargetMegapixels.Value!);
        }

        // Optional: expose programmatic access to the loaded image(s)
        public BitmapSource? GetImageForColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex > 2) throw new ArgumentOutOfRangeException(nameof(columnIndex));
            return _columnBitmaps[columnIndex];
        }

        private void CallSelectedPipelineWithErrorHandling()
        {
            if (_columnBitmaps[0] != null)
            {
                CallSelectedPipeline();
            }
        }

        private void RenderButtonClick(object sender, RoutedEventArgs e)
        {
            CallSelectedPipelineWithErrorHandling();
        }


    }
}