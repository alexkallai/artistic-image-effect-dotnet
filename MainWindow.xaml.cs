using artistic_image_effect_dotnet;
using Microsoft.Win32;
using System.IO;
using System.Windows;
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

                CanvasPipeline();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to load image: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CanvasPipeline()
        {
            canvas = new bool[loadedImageHeight, loadedImageWidth];

            ImageEffect.DrawCircle(ref canvas, 10, 10, 5, 1);

            SetImageSourceForColumn(1, ImageEffect.CreateWriteableBitmapFromArray(canvas));
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
    }
}