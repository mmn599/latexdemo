using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
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

        private string runPredictScript(string scriptfn, string clffn, string pythonfn, string imagefn)
        {
            var process = new Process();
            process.StartInfo.FileName = pythonfn;
            process.StartInfo.Arguments = scriptfn + " " + imagefn + " " + clffn;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string latexoutput = process.StandardOutput.ReadToEnd();
            return latexoutput;
        }

        private static string ROOT_DIR = "C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\";
        private static string DOODLE_FN = ROOT_DIR + "doodle.png";
        private static string LATEX_FN = ROOT_DIR + "mylatex.tex";
        private static string BATCH_FN = ROOT_DIR + "batch.bat";
        private static string LATEXIMAGE_FN = ROOT_DIR + "output.png";
        private static string PREDICTSCRIPT_FN = "C:\\Users\\mmnor\\Projects\\autolatex\\predict.py";
        private static string CLF_FN = "C:/Users/mmnor/Projects/autolatex/MLP.p";
        private static string PYTHON_FN = "C:\\Anaconda3\\python.exe";

        private void displayLatex(string latexfn, string batchfn, string lateximagefn, string text)
        {
            File.WriteAllText(latexfn, text);
            var p = new Process();
            var sb = new StringBuilder();
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.ErrorDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.StartInfo.FileName = batchfn;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            var latexOutputImage = new BitmapImage(new Uri(lateximagefn, UriKind.Absolute));
            imageLatex.Source = latexOutputImage;
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
            saveCanvas(DOODLE_FN);
            string latex = runPredictScript(PREDICTSCRIPT_FN, CLF_FN, PYTHON_FN, DOODLE_FN);
            displayLatex(LATEX_FN, BATCH_FN, LATEXIMAGE_FN, latex);
        }
    }
}
