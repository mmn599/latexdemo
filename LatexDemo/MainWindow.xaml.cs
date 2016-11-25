using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
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

        private string runPredictScript(string imagefn, int version, int predict_count)
        {
            var process = new Process();
            process.StartInfo.FileName = PYTHON_FN;
            process.StartInfo.Arguments = PREDICTSCRIPT_FN + " " + imagefn + " " + version + " " + predict_count;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            string scriptoutput = process.StandardOutput.ReadToEnd();
            string erroroutput = process.StandardError.ReadToEnd();
            Debug.Write(erroroutput);
            return scriptoutput;
        }

        private static string ROOT_DIR = "C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\";
        private static string TEMP_DIR = ROOT_DIR + "\\temp\\";
        private static string DOODLE_FN = TEMP_DIR + "doodle.png";
        private static string LATEX_FN = TEMP_DIR + "mylatex.tex";
        private static string OUTPUTPDF_FN = TEMP_DIR + "output.pdf";
        private static string BATCH_FN = TEMP_DIR + "batch.bat";
        private static string LATEXIMAGE_FN = ROOT_DIR + "output.png";
        private static string PREDICTSCRIPT_FN = "C:\\Users\\mmnor\\Projects\\autolatex\\predict.py";
        private static string PYTHON_FN = "C:\\Anaconda3\\python.exe";
        private static string PICTURE_FN = "C:\\Users\\mmnor\\Projects\\LatexDemo\\LatexDemo\\temp\\output";

        private void displayLatex(string text)
        {
            File.WriteAllText(LATEX_FN, text);
            var p = new Process();
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            string picturefn = PICTURE_FN + PREDICT_COUNT + ".png";
            p.ErrorDataReceived += (sender, args) => Debug.Write(args.Data + "\n");
            p.OutputDataReceived += (sender, args) => Debug.Write(args.Data + "\n");
            p.StartInfo.FileName = BATCH_FN;
            string latexFnRemoved = LATEX_FN.Replace(".tex", "");
            p.StartInfo.Arguments = picturefn + " " + latexFnRemoved + " " + OUTPUTPDF_FN;
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

        private void reset()
        {
            //ink.Strokes.Clear();
            imageLatex.Visibility = Visibility.Collapsed;
            imageProcess.Visibility = Visibility.Collapsed;
            btnReset.Visibility = Visibility.Collapsed;
            ink.Visibility = Visibility.Visible;
            btnPredict.Visibility = Visibility.Visible;
        }

        private void btnResetClick(object sender, RoutedEventArgs e)
        {
            reset();
        }

        private int SAVE_COUNT = 1000;
        private int PREDICT_COUNT = 0;

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            string symbol = textTrue.Text;
            string dir = @"C:\Users\mmnor\Projects\autolatex\data\MYDATA\train\";
            string fileName = dir + SAVE_COUNT + "_" + symbol + ".png";
            saveCanvas(fileName);
            SAVE_COUNT += 1;
            reset();
        }

        int CURRENT_VERSION = 2;

        private void btnPredictClick(object sender, RoutedEventArgs e)
        {
            btnPredict.Visibility = Visibility.Collapsed;
            saveCanvas(DOODLE_FN);
            //ink.Visibility = Visibility.Collapsed;

            string scriptoutput = runPredictScript(DOODLE_FN, CURRENT_VERSION, PREDICT_COUNT);
            string latex = "default";
            string labelsImageFn = "default";
            int numSymbols = -1;
            var symbolImgFileNames = new List<string>();
            using (var reader = new StringReader(scriptoutput))
            {
                latex = reader.ReadLine();
                labelsImageFn = reader.ReadLine();
                numSymbols = Int32.Parse(reader.ReadLine());
                for(int i = 0; i < numSymbols; i++)
                {
                    string symbolImgFn = reader.ReadLine();
                    symbolImgFileNames.Add(symbolImgFn);
                }
            }

            textPred.Text = latex;

            listSymbolImages.Items.Clear();
            foreach(var symbolImgFileName in symbolImgFileNames)
            {
                var magicImage = new MagicImage();
                magicImage.Path = symbolImgFileName;
                listSymbolImages.Items.Add(magicImage);
            }

            /*
            string latexDocument = LATEX_PRE + latex + LATEX_POST;
            displayLatex(latexDocument);

            var processedOutputImage = new BitmapImage(new Uri(labelsImageFn, UriKind.Absolute));
            imageProcess.Source = processedOutputImage;

            //imageProcess.Visibility = Visibility.Visible;
            imageLatex.Visibility = Visibility.Visible;
            */

            btnReset.Visibility = Visibility.Visible;

            PREDICT_COUNT += 1;
        }
    }
}

public class MagicImage
{
    public string Path { get; set; }
}
