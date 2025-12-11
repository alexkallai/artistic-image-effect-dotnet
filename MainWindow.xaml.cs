using artistic_image_effect_dotnet;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
                LoadFileToColumn(dlg.FileName, 0);
            }
        }

        private void LoadFileToColumn(string path, int columnIndex)
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
                loadedImageWidth = Convert.ToInt32(bi.Width);
                loadedImageHeight = Convert.ToInt32(bi.Height);

                _columnBitmaps[columnIndex] = bi;
                SetImageSourceForColumn(columnIndex, bi);

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
                { "Concentric circles", CanvasPipeline},
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

        private void CanvasPipeline()
        {
            canvas = new bool[loadedImageHeight, loadedImageWidth];

            // Do the pipeline based on the selected option

            for (int i = 0; i < 120; i++)
            {
                ImageEffect.DrawCircle(ref canvas, Convert.ToInt32(loadedImageWidth / 2), 0, i * 10, 2, false);
            }
            var precalculatedBitmap = new WriteableBitmap(_columnBitmaps[0]);
            var drawnCanvas = ImageEffect.DrawFilledCirclesWidthSizes(canvas, precalculatedBitmap);

            var bitmapCanvas = ImageEffect.CreateWriteableBitmapFromArray(canvas);
            _columnBitmaps[1] = bitmapCanvas;
            SetImageSourceForColumn(1, bitmapCanvas);

            var drawnBitmapCanvas = ImageEffect.CreateWriteableBitmapFromArray(drawnCanvas);
            _columnBitmaps[2] = drawnBitmapCanvas;
            SetImageSourceForColumn(2, drawnBitmapCanvas);
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
            LoadFileToColumn(files[0], 0);
        }

        // Optional: expose programmatic access to the loaded image(s)
        public BitmapSource? GetImageForColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex > 2) throw new ArgumentOutOfRangeException(nameof(columnIndex));
            return _columnBitmaps[columnIndex];
        }
        private void GlobalControlTabs_SelectedIntecChanged(Object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(GlobalControlTabs.SelectedItem);
            if (_columnBitmaps[0] != null)
            {
                CallSelectedPipeline();
            }
        }

    }
}