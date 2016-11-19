using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LatexDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnPredict.Click += btnPredictClick;
        }

        private void runScript()
        {
            string cmd = "C:\\Users\\mmnor\\Projects\\autolatex\\predict.py";
            string imageFileName = "C:/Users/mmnor/Projects/autolatex/poopy.png";
            string clfLocation = "C:/Users/mmnor/Projects/autolatex/MLP.p";
            string pythonName = "C:\\Anaconda3\\python.exe";
            Process process = new Process();
            process.StartInfo.FileName = pythonName;
            process.StartInfo.Arguments = cmd + " " + imageFileName + " " + clfLocation;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            textBox.Text = output;
        }

        private void saveCanvas(string fullFileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(ink);
            double dpi = 96d;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(ink);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            using (MemoryStream ms = new System.IO.MemoryStream())
            {
                pngEncoder.Save(ms);
                File.WriteAllBytes(fullFileName, ms.ToArray());
            }
        }

        private void btnPredictClick(object sender, RoutedEventArgs e)
        {
            string fileName = "C:\\Users\\mmnor\\Projects\\LatexDemo\\yeet.png";
            saveCanvas(fileName);
            runScript();
        }
    }
}
