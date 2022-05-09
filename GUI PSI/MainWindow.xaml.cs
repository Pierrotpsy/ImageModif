using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using PSI;
using Microsoft.Win32;

namespace GUI {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MyImage modifImage;
        private int a = 0;
        private string path;
        private string wd;
        private bool imported = false;
        private MyImage startImage;
        public MainWindow() {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.bmp)|*.*";
            dlg.RestoreDirectory = true;
            wd = Directory.GetCurrentDirectory();
            if (dlg.ShowDialog() == true) {
                path = dlg.FileName;
                this.modifImage = new MyImage(path);
                this.startImage = new MyImage(this.modifImage);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                ImageViewer1.Source = bitmap;
                imported = true;
            }
        }
        
        private void Reset(object sender, RoutedEventArgs e) {
            if(imported) {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                ImageViewer1.Source = bitmap;
                this.modifImage = new MyImage(startImage);
            }
        }

        private void openFile(string txt) {

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(txt);
                bitmap.EndInit();
                ImageViewer1.Source = bitmap;
        }
        private void Rot(object sender, RoutedEventArgs e) {
            double n;
            if(double.TryParse(Rotation.Text, out n)) {
                this.modifImage.Rotation(n);
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            } n = 0;
            
        }

        private void Rot90(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.rotation90();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void Contrast(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.increase_contrast();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void EdgeDet(object sender, RoutedEventArgs e) {
            int n;
            if (int.TryParse(Edge.Text, out n)) {
                this.modifImage.edgedetection(n);
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
            
        }

        private void Mandelbrot(object sender, RoutedEventArgs e) {
            this.modifImage = new MyImage("coco.bmp");
            this.modifImage.MandelbrotSet(400, 600, 50);
            this.modifImage.MyImageToFile("modifImage.bmp");
            this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
            openFile(wd + "\\modifImage" + a + ".bmp");
            a++;
        }

        private void Mirror(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.Mirror();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void GrayScale(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.ToGrayScale();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void Fuzzy(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.fuzzy();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void Embossing(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.embossing();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void EdgeRefor(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.edge_reforming();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void Histo(object sender, RoutedEventArgs e) {
            if (imported) {
                Histogram histo = new Histogram(this.modifImage);
                MyImage i = new MyImage(histo);
                i.MyImageToFile("Histogram.bmp");
                Process.Start("Histogram.bmp");
            }
        }

        private void BandW(object sender, RoutedEventArgs e) {
            if (imported) {
                this.modifImage.ToBlackAndWhite();
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void Scale(object sender, RoutedEventArgs e) {
            double n;
            if (double.TryParse(Resize.Text, out n)) {
                this.modifImage.Resize(n);
                this.modifImage.MyImageToFile("modifImage" + a + ".bmp");
                openFile(wd + "\\modifImage" + a + ".bmp");
                a++;
            }
        }

        private void QR(object sender, RoutedEventArgs e) {
            if (QRcode.Text != null && QRcode.Text != "") {
                string s = QRcode.Text;
                QRcode qr = new QRcode(s, 'L');
                MyImage Q = new MyImage(qr);
                Q.Resize(8);
                Q.MyImageToFile("qrcode.bmp");
                Process.Start("qrcode.bmp");
            }
        }
    }
}

