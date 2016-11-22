using System;
using System.Diagnostics;
using System.IO;
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
            CenterWindowOnScreen();
            btnPredict.Click += btnPredictClick;
            btnReset.Click += btnResetClick;
            btnSave.Click += btnSaveClick;
        }

        private string runPredictScript(string scriptfn, string clffn, string ftfn, string pythonfn, string imagefn)
        {
            var process = new Process();
            process.StartInfo.FileName = pythonfn;
            process.StartInfo.Arguments = scriptfn + " " + imagefn + " " + clffn + " " + ftfn;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            string latexoutput = process.StandardOutput.ReadToEnd();
            string erroroutput = process.StandardError.ReadToEnd();
            Debug.Write(erroroutput);
            return latexoutput;
        }

        private static string ROOT_DIR = "C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\";
        private static string DOODLE_FN = ROOT_DIR + "doodle.png";
        private static string LATEX_FN = ROOT_DIR + "mylatex.tex";
        private static string BATCH_FN = ROOT_DIR + "batch.bat";
        private static string LATEXIMAGE_FN = ROOT_DIR + "output.png";
        private static string PREDICTSCRIPT_FN = "C:\\Users\\mmnor\\Projects\\autolatex\\predict.py";
        private static string CLF_FN = "C:/Users/mmnor/Projects/autolatex/MLPCustom2.p";
        private static string FT_FN = "C:/Users/mmnor/Projects/autolatex/PCACustom2.p";
        private static string PYTHON_FN = "C:\\Anaconda3\\python.exe";
        private static string PICTURE_FN = "C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\output";

        private void displayLatex(string latexfn, string batchfn, string text)
        {
            File.WriteAllText(latexfn, text);
            var p = new Process();
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            string picturefn = PICTURE_FN + PREDICT_COUNT + ".png";
            p.StartInfo.Arguments = picturefn;
            p.ErrorDataReceived += (sender, args) => Debug.Write(args.Data + "\n");
            p.OutputDataReceived += (sender, args) => Debug.Write(args.Data + "\n");
            p.StartInfo.FileName = batchfn;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            var latexOutputImage = new BitmapImage(new Uri(picturefn, UriKind.Absolute));
            imageLatex.Source = latexOutputImage;
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void saveCanvas(string fullFileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(ink);
            double dpi = 96d;
            var rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(ink);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var ms = new MemoryStream())
            {
                pngEncoder.Save(ms);
                File.WriteAllBytes(fullFileName, ms.ToArray());
            }
        }

        private static string LATEX_PRE = "\\documentclass[preview,border=12pt]{standalone}\n\\usepackage{amsmath}\n\\usepackage{tikz}\n\\begin{document}\n$";
        private static string LATEX_POST = "$\n\\end{document}";

        private void btnResetClick(object sender, RoutedEventArgs e)
        {
            ink.Strokes.Clear();
            imageLatex.Visibility = Visibility.Collapsed;
            ink.Visibility = Visibility.Visible;
            btnReset.Visibility = Visibility.Collapsed;
            btnPredict.Visibility = Visibility.Visible;
        }

        private int SAVE_COUNT = 0;
        private int PREDICT_COUNT = 0;

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            string symbol = textBox.Text;
            string dir = @"C:\Users\mmnor\Projects\autolatex\data\MYDATA\train\";
            string fileName = dir + SAVE_COUNT + "_" + symbol + ".png";
            saveCanvas(fileName);
            SAVE_COUNT += 1;
            ink.Strokes.Clear();
        }

        private void btnPredictClick(object sender, RoutedEventArgs e)
        {
            btnPredict.Visibility = Visibility.Collapsed;
            saveCanvas(DOODLE_FN);
            ink.Visibility = Visibility.Collapsed;
            string latex = runPredictScript(PREDICTSCRIPT_FN, CLF_FN, FT_FN, PYTHON_FN, DOODLE_FN);
            textBox.Text = latex;
            string latexDocument = LATEX_PRE + latex + LATEX_POST;
            displayLatex(LATEX_FN, BATCH_FN, latexDocument);
            imageLatex.Visibility = Visibility.Visible;
            btnReset.Visibility = Visibility.Visible;
            PREDICT_COUNT += 1;
        }
    }
}
