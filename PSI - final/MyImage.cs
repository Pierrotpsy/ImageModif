using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Design;
using System.IO;

namespace PSI
{
    public class MyImage
    {
        private string type;
        private int fileSize;
        private int offsetSize;
        private int height;
        private int width;
        private int bitsPerPixel;
        public RGB[,] image;

        private byte[] compression;
        private byte[] imageSize;
        private byte[] XpixelsPerM;
        private byte[] YpixelsPerM;
        private byte[] ColorsUsed;
        private byte[] ImportantColors;
        private int size;

        /// <summary>
        /// Constructeur de MyImage prenant en paramètre le chemin vers un fichier bmp pour lire ses données.
        /// </summary>
        /// <param name="file"></param>
        public MyImage(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);

            this.size = bytes.Length;
            this.type = ((char)bytes[0]).ToString() + (char)bytes[1];
            /*for (int g = 0; g < 54; g++) {
                Console.Write(bytes[g]);
            }*/
            this.fileSize = LEToInt(subArray(bytes, 2, 5));
            this.offsetSize = LEToInt(subArray(bytes, 10, 13));
            this.width = LEToInt(subArray(bytes, 18, 21));
            this.height = LEToInt(subArray(bytes, 22, 25));
            this.bitsPerPixel = LEToInt(subArray(bytes, 28, 29));
            this.compression = subArray(bytes, 31, 34);
            this.imageSize = subArray(bytes, 35, 38);
            this.XpixelsPerM = subArray(bytes, 39, 42);
            this.YpixelsPerM = subArray(bytes, 43, 46);
            this.ColorsUsed = subArray(bytes, 47, 50);
            this.ImportantColors = subArray(bytes, 51, 54);
            //test(bytes);
            int a = 0;
            int b = 0;
            //Console.WriteLine(height);
            image = new RGB[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    image[i, j] = RGB.FromRGB(bytes[54 + a + b], bytes[54 + 1 + a + b], bytes[54 + 2 + a + b]);
                    b += 3;
                }
                b = 0;

                a += width * 3;
            }
        }

        /// <summary>
        /// Constructeur de MyImage prenant en paramètre un QRcode
        /// </summary>
        /// <param name="qr"></param>
        public MyImage(QRcode qr) {

            this.image = qr.image;
            int l = image.GetLength(0);
            RGB[,] newImage = new RGB[l+12-l%4, l+12-l%4];
            //Console.WriteLine(l +l- l % 4);
            for(int i = 0; i < newImage.GetLength(0); i++) {
                for(int j = 0; j < newImage.GetLength(0); j++) {
                    if (i > 3 && j > 3 && i < l+4 && j < l+4) {
                        newImage[i, j] = image[i-4, j-4];
                    } else newImage[i, j] = RGB.FromRGB(255, 255, 255);

                }
            }
            image = newImage;

            /*for (int i = 0; i < image.GetLength(0); i++) {
                for (int j = 0; j < image.GetLength(0); j++) {
                    Console.Write(image[i, j] + "  ");
                }
                Console.WriteLine();
            }*/
            this.type = "BM";
            byte[] b = { 230, 4, 0, 0 };
            this.fileSize = LEToInt(b);
            this.offsetSize = 54;
            this.width = image.GetLength(0);
            this.height = image.GetLength(0);
            this.size = width*height*3 + 54;
            this.bitsPerPixel = 24;
            this.compression = intToLE(0);
            this.imageSize = intToLE(0);
            this.XpixelsPerM = intToLE(0);
            this.YpixelsPerM = intToLE(0);
            this.ColorsUsed = intToLE(0);
            this.ImportantColors = intToLE(0);
            rotation90();
        }

        /// <summary>
        /// Ce constructeur est utile pour effectuer les test unitaires.
        /// </summary>
        public MyImage() { }

        /// <summary>
        /// Ce constructeur permet de créer une image de l'histogramme d'une image
        /// </summary>
        /// <param name="histo"></param>
        public MyImage(Histogram histo) {

            this.image = histo.graph;

            this.type = "BM";
            byte[] b = { 230, 4, 0, 0 };
            this.fileSize = LEToInt(b);
            this.offsetSize = 54;
            this.width = image.GetLength(1);
            this.height = image.GetLength(0);
            this.size = width * height * 3 + 54;
            this.bitsPerPixel = 24;
            this.compression = intToLE(0);
            this.imageSize = intToLE(0);
            this.XpixelsPerM = intToLE(0);
            this.YpixelsPerM = intToLE(0);
            this.ColorsUsed = intToLE(0);
            this.ImportantColors = intToLE(0);

            rotation90();
            rotation90();

        }

        /// <summary>
        /// Contructeur de copie de MyImage
        /// </summary>
        /// <param name="copy"></param>
        public MyImage(MyImage copy) {
            this.size = copy.size;
            this.type = copy.type;
            this.fileSize = copy.fileSize;
            this.offsetSize = copy.offsetSize;
            this.width = copy.width;
            this.height = copy.height;
            this.bitsPerPixel = copy.bitsPerPixel;
            this.compression = copy.compression;
            this.imageSize = copy.imageSize;
            this.XpixelsPerM = copy.XpixelsPerM;
            this.YpixelsPerM = copy.YpixelsPerM;
            this.ColorsUsed = copy.ColorsUsed;
            this.ImportantColors = copy.ImportantColors;
            this.image = new RGB[this.height, this.width];
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    image[i, j] = copy.image[i, j];
                }
            }
        }

        /// <summary>
        /// Fonction pour afficher 
        /// </summary>
        /// <param name="myfile"></param>
        public void test(byte[] myfile)
        {
            Console.WriteLine("\n Header \n");
            for (int i = 0; i < 14; i++)
                Console.Write(myfile[i] + " ");
            //Métadonnées de l'image
            Console.WriteLine("\n HEADER INFO \n");
            for (int i = 14; i < 54; i++)
                Console.Write(myfile[i] + " ");
            //L'image elle-même
            Console.WriteLine("\n IMAGE \n");
            for (int i = 54; i < myfile.Length; i = i + width*3)
            {
                for (int j = i; j < i + width*3; j++)
                {
                    Console.Write(myfile[j] + " ");

                }
                Console.WriteLine();
            }
        }
        /// <summary>
        /// Cette méthode traduit un byte en little indian en entier
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int LEToInt(byte[] b)
        {
            if (b.Length == 4)
            {
                return (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];

            }
            else if (b.Length == 2)
            {
                return (b[1] << 8) | b[0];
            }
            else return 0;
        }

        /// <summary>
        /// Cette méthode passe l'image en nuance de gris
        /// </summary>
        public void ToGrayScale()
        {
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    byte gray = (byte)(0.21 * image[i, j].R + 0.71 * image[i, j].G + 0.071 * image[i, j].B);
                    image[i, j] = RGB.FromRGB(gray, gray, gray);
                }
            }
        }
        /// <summary>
        /// cette méthode passe l'image en noir et blanc
        /// </summary>
        public void ToBlackAndWhite()
        {
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    int average = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                    if (average < 200)
                    {
                        image[i, j] = RGB.FromRGB(0, 0, 0);
                    }
                    else
                    {
                        image[i, j] = RGB.FromRGB(255, 255, 255);
                    }
                }
            }
        }
        /// <summary>
        /// Cette méthode transforme l'image en une image miroire
        /// </summary>
        public void Mirror()
        {
            RGB[,] newImage = new RGB[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    newImage[i, j] = image[i, width - 1 - j];
                }
            }
            image = newImage;
        }
        /// <summary>
        /// Cette méthode converti un entier en un entier mutiple de 4
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private int ConvertMultiple4(int number)
        {
            int r = number % 4;
            if (r != 0)
            {
                number -= r;
            }
            return number;
        }
        /// <summary>
        /// Cette méthode permet de modifier les dimensions d'une image
        /// </summary>
        /// <param name="factor"></param>
        public void Resize(double factor)
        {

            RGB[,] newImage = new RGB[ConvertMultiple4((int)(factor * height)), ConvertMultiple4((int)(factor * width))];
            //Console.WriteLine(newImage.GetLength(0));
            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    newImage[i, j] = image[(int)Math.Floor(i / factor), (int)Math.Floor(j / factor)];
                }
            }
            height = newImage.GetLength(0);
            width = newImage.GetLength(1);
            size = 54 + newImage.GetLength(0) * newImage.GetLength(1) * 3;
            image = newImage;
        }

        /// <summary>
        /// Cette méthode permet de tourner une image d'un angle précis entré en paramètre dans le sens horaire
        /// </summary>
        /// <param name="angle"></param>
        public void Rotation(double angle) {
            RGB[,] modifMat = new RGB[image.GetLength(0), image.GetLength(1)];

            for (int i = 0; i < modifMat.GetLength(0); i++) {
                for (int j = 0; j < modifMat.GetLength(1); j++) {
                    modifMat[i, j] = RGB.FromRGB(0, 0, 0);
                }
            }

            for (int i = 0; i < modifMat.GetLength(0); i++) {
                for (int j = 0; j < modifMat.GetLength(1); j++) {
                    int xMilieu = (int)modifMat.GetLength(1) / 2;
                    int yMilieu = (int)modifMat.GetLength(0) / 2;
                    double angleRad = (Math.PI / 180) * angle;

                    int dX = j - xMilieu;
                    int dY = i - yMilieu;

                    int x = (int)(Math.Cos(angleRad) * dX - Math.Sin(angleRad) * dY) + xMilieu;
                    int y = (int)(Math.Sin(angleRad) * dX + Math.Cos(angleRad) * dY) + yMilieu;

                    if ((x >= 0) && (x < modifMat.GetLength(1)) && (y >= 0) && (y < modifMat.GetLength(0))) {
                        byte valRouge = image[y, x].R;
                        byte valVert = image[y, x].G;
                        byte valBleu = image[y, x].B;
                        modifMat[i, j] = RGB.FromRGB(valRouge, valVert, valBleu);
                    }
                }
            }
            image = modifMat;
        }
        /// <summary>
        ///  Méthode qui tourne une image de 90 dégré dans le sens horaire
        /// </summary>
        public void rotation90() {
            RGB[,] newImage = new RGB[image.GetLength(1), image.GetLength(0)];
            for (int j = 0; j < image.GetLength(1); j++) {
                for (int i = 0; i < image.GetLength(0); i++) {
                    newImage[image.GetLength(1) - j - 1, i] = image[i, j];
                }
            }

            this.image = newImage;

        }
        /// <summary>
        /// Cette méthode ajoute des 0 afin d'atteindre une taille de byte voulue
        /// cette taille de byte est entrée en paramètre
        /// Cette méthode agit sur un tableau de bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="expectedSize"></param>
        /// <returns></returns>
        private byte[] formatBytes(byte[] bytes, int expectedSize)
        {
            byte[] newBytes = new byte[expectedSize];
            if (bytes.Length < expectedSize)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    newBytes[i] = bytes[i];
                }

                for (int i = bytes.Length; i < expectedSize; i++)
                {
                    newBytes[i] = 0;
                }
            }
            else if (bytes.Length > expectedSize)
            {
                for (int i = 0; i < expectedSize; i++)
                {
                    newBytes[i] = bytes[i];
                }
            }

            return newBytes;
        }
        /// <summary>
        /// Cette méthode permet de prendre une partie d'un tableau et d'en créer un nouveau qui est égal à la partie du premier tableau
        /// </summary>
        /// <param name="startArray"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private byte[] subArray(byte[] startArray, int start, int end)
        {
            byte[] newArray = new byte[end + 1 - start];
            int a = 0;
            for (int i = start; i < end + 1; i++)
            {
                newArray[a] = startArray[i];
                a++;
            }
            return newArray;
        }
        /// <summary>
        /// Méthode qui converti un entier en byte sous le format little indian
        /// </summary>
        /// <param name="i"></param>
        /// <param name="expectedSize"></param>
        /// <returns></returns>
        public byte[] intToLE(int i, int expectedSize = 4)
        {
            string s = "";
            byte[] bytes = BitConverter.GetBytes(i);
            if (bytes.Length < expectedSize || bytes.Length > expectedSize)
            {
                bytes = formatBytes(bytes, expectedSize);
            }
            for (int b = 0; b < bytes.Length; b++)
            {
                s += bytes[b];
            }
            return bytes;
        }
        /// <summary>
        /// Méthode qui converti un tableau de bytes en un unique string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string byteToString(byte[] bytes)
        {
            string s = "";
            for (int b = 0; b < bytes.Length; b++)
            {
                s += bytes[b];
            }
            return s;
        }

        /// <summary>
        /// Cett méthode permet de remplir un tableau de bytes avec un byte choisit et à partir d'un index
        /// Le byte choisit 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="filler"></param>
        /// <param name="start"></param>
        private void fillArray(ref byte[] bytes, byte[] filler, int start)
        {
            for (int i = 0; i < filler.Length; i++)
            {
                bytes[start + i] = filler[i];
            }
        }

        /// <summary>
        /// Cette méthode permet d'enregistrer une image
        /// Le nom donné à cette image correspond au paramètre file
        /// </summary>
        /// <param name="file"></param>
        public void MyImageToFile(string file)
        {
            
            byte[] bytes = new byte[size];
            bytes[0] = Convert.ToByte(type[0]);
            bytes[1] = Convert.ToByte(type[1]);
            fillArray(ref bytes, intToLE(fileSize), 2);
            fillArray(ref bytes, intToLE(0), 6);
            fillArray(ref bytes, intToLE(offsetSize), 10);
            fillArray(ref bytes, intToLE(40), 14);
            fillArray(ref bytes, intToLE(width), 18);
            fillArray(ref bytes, intToLE(height), 22);
            fillArray(ref bytes, intToLE(1, 2), 26);
            fillArray(ref bytes, intToLE(bitsPerPixel, 2), 28);
            fillArray(ref bytes, compression, 31);
            fillArray(ref bytes, intToLE(0, 1), 34);
            fillArray(ref bytes, imageSize, 35);
            fillArray(ref bytes, XpixelsPerM, 39);
            fillArray(ref bytes, YpixelsPerM, 43);
            fillArray(ref bytes, ColorsUsed, 47);
            fillArray(ref bytes, intToLE(0, 3), 51);

            int a = 0;
            //Console.WriteLine(image.Length);
            foreach (RGB c in image)
            {
                bytes[54 + a] = intToLE(c.R)[0];
                bytes[54 + 1 + a] = intToLE(c.G)[0];
                bytes[54 + 2 + a] = intToLE(c.B)[0];
                a += 3;
            }


            File.WriteAllBytes(file, bytes);
        }
        /// <summary>
        /// Cette méthode crée une fractale de Mandelbrot en faisant appel à la méthode de calul Mandelbrot
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="maxIterations"></param>
        public void MandelbrotSet(int height, int width, int maxIterations)
        {

            int RE_START = -2;
            int RE_END = 1;
            int IM_START = -1;
            int IM_END = 1;
            RGB[,] mandelbrot = new RGB[height, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Complex c = new Complex(RE_START + ((double)i / width) * (RE_END - RE_START), IM_START + ((double)j / height) * (IM_END - IM_START));
                    double m = Mandelbrot(c, maxIterations);
                    int hue = (int)(255 * m / maxIterations);
                    int saturation = 255;
                    int value = 0;
                    if (m < maxIterations)
                    {
                        value = 255;
                    }
                    mandelbrot[j, i] = HSVToRGB(hue, saturation, value);
                }
            }
            image = mandelbrot;
            size = 54 + height * width * 3;
            this.height = height;
            this.width = width;
        }
        /// <summary>
        /// Méthode de calcul pour la fractale de mandelbrot
        /// </summary>
        /// <param name="c"></param>
        /// <param name="maxIterations"></param>
        /// <returns></returns>
        private double Mandelbrot(Complex c, int maxIterations)
        {
            Complex z = new Complex(0, 0);
            int n = 0;
            while (Complex.Modulus(z) <= 2 && n < maxIterations)
            {
                z = z * z + c;
                n += 1;
            }

            if (n == maxIterations) return maxIterations;


            return n + 1 - Math.Log(Math.Log(Complex.Modulus(z)) / Math.Log(2));
        }
        /// <summary>
        /// Cette méthode convertie une couleur de norme HSV en RGB
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns>
        /// La valeur de la couleur en RGB
        /// </returns>
        private RGB HSVToRGB(int hue, int saturation, int value)
        {
            double hh, p, q, t, ff;
            long i;
            RGB rgb = new RGB();

            if (saturation <= 0.0)
            {       // < is bogus, just shuts up warnings
                rgb.R = (byte)value;
                rgb.G = (byte)value;
                rgb.B = (byte)value;
                return rgb;
            }
            hh = hue;
            if (hh >= 360.0) hh = 0.0;
            hh /= 60.0;
            i = (long)hh;
            ff = hh - i;
            p = value * (1.0 - saturation);
            q = value * (1.0 - (saturation * ff));
            t = value * (1.0 - (saturation * (1.0 - ff)));

            switch (i)
            {
                case 0:
                    rgb.R = (byte)value;
                    rgb.G = (byte)(int)t;
                    rgb.B = (byte)(int)p;
                    break;
                case 1:
                    rgb.R = (byte)(int)q;
                    rgb.G = (byte)value;
                    rgb.B = (byte)(int)p;
                    break;
                case 2:
                    rgb.R = (byte)(int)p;
                    rgb.G = (byte)value;
                    rgb.B = (byte)(int)t;
                    break;

                case 3:
                    rgb.R = (byte)(int)p;
                    rgb.G = (byte)(int)q;
                    rgb.B = (byte)value;
                    break;
                case 4:
                    rgb.R = (byte)(int)t;
                    rgb.G = (byte)(int)p;
                    rgb.B = (byte)value;
                    break;
                case 5:
                default:
                    rgb.R = (byte)value;
                    rgb.G = (byte)(int)p;
                    rgb.B = (byte)(int)q;
                    break;
            }
            return rgb;
        }
        /// <summary>
        /// Cette méthode permet de réalisé une convolution en prenant comme noyau de convolution le paramètre kernel
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="modifiedimage"></param>
        public void Convolution(double[,] kernel, RGB[,] modifiedimage)
        {
            int r = (int)Math.Floor(kernel.GetLength(0) / 2.0);
            int a = 0;
            int b = 0;
            int sum = 0;
            foreach (int i in kernel) sum += i;
            if (sum == 0) sum = 1;


            RGB[,] temp = new RGB [modifiedimage.GetLength(0),modifiedimage.GetLength(1)];
            for (int i=0; i<modifiedimage.GetLength(0);i++)
            {
                for (int j=0; j<modifiedimage.GetLength(1);j++)
                {
                    temp[i, j] = RGB.FromRGB(CheckRGB((byte)(0)), CheckRGB((byte)(0)), CheckRGB((byte)(0)));
                }
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double R = 0;
                    double G = 0;
                    double B = 0;
                    for (int k = 0; k < kernel.GetLength(0); k++)
                    {
                        for (int l = 0; l < kernel.GetLength(1); l++)
                        {
                            a = i - r + k;
                            b = j - r + l;


                            if (a < 0)
                            {
                                a += modifiedimage.GetLength(0);
                            }
                            if (a > modifiedimage.GetLength(0) - 1)
                            {
                                a -= modifiedimage.GetLength(0);
                            }
                            if (b < 0)
                            {
                                b += modifiedimage.GetLength(1);
                            }
                            if (b > modifiedimage.GetLength(1) - 1)
                            {
                                b -= modifiedimage.GetLength(1);
                            }
                            //Console.WriteLine(a + " " + b);
                            R += Convert.ToDouble(image[a, b].R) * kernel[k, l];
                            G += Convert.ToDouble(image[a, b].G) * kernel[k, l];
                            B += Convert.ToDouble(image[a, b].B) * kernel[k, l];

                        }
                        //Console.WriteLine(s.R + " " + s.G + " " + s.B);
                    }
                    
                    if (R < 0) R = 0;
                    if (G < 0) G = 0;
                    if (B < 0) B = 0;
                    if (R > 255) R = 255;
                    if (G > 255) G = 255;
                    if (B > 255) B = 255;
                    

                    Convert.ToInt32(R);
                    Convert.ToInt32(G);
                    Convert.ToInt32(B);

                    //Console.WriteLine(""+R, G, B);
                    temp[i, j] = RGB.FromRGB(CheckRGB((byte)(R)), CheckRGB((byte)(G)), CheckRGB((byte)(B)));


                    //modifiedimage[i, j] = RGB.FromRGB(((byte)(R / sum)), (byte)(G / sum), (byte)(B / sum));
                }
            }
            image = temp;
        }
        /// <summary>
        /// Cette méthode permet de vérifier que la valeur de RGB entrée est bien comprise entre 255 et 0
        /// si la valeur est supérieure à 255 on lui assigne la valeur 255
        /// si la valeur est inférieure à 0 on lui assigne la valeur 0
        /// </summary>
        /// <param name="n"></param>
        /// <returns>
        /// le byte modifié
        /// </returns>
        private byte CheckRGB(byte n)
        {
            byte a = n;
            if (n > 255) a = 255;
            if (n < 0) a = 0;
            return a;
        }
        /// <summary>
        /// Cette méthode retourne le niveau de mise en valeur des contours voulu en fonction paramètre level entré
        /// Elle fait ensuite appel à la méthode convolution en appliquant la bonne matrice kernel
        /// </summary>
        /// <param name="level"></param>
        public void edgedetection(int level) {
            // différent niveaux de contours
            switch (level) {
                case 1:
                    double[,] kernel1 = { { 1, 0, -1 }, { 0, 0, 0 }, { -1, 0, 1 } };
                    Convolution(kernel1, this.image);
                    break;

                case 2:
                    double[,] kernel2 = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
                    Convolution(kernel2, this.image);
                    break;
                case 3:
                    double[,] kernel3 = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
                    Convolution(kernel3, this.image);
                    break;
                default:
                    double[,] kernel4 = { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
                    Convolution(kernel4, this.image);
                    break;

            }
        }
        /// <summary>
        /// Cette méthode fait appel à la méthode conculution en appliquant un kernel qui permet de flouter l'image
        /// </summary>
        public void fuzzy()
        {
            // flou 
            //int[,] kernel = new int[,] { { 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 0, 0, 0, 0 } };
            double[,] kernel = new double[,] { { 1 / 9.0, 1 / 9.0, 1 / 9.0 }, { 1 / 9.0, 1 / 9.0, 1 / 9.0 }, { 1 / 9.0, 1 / 9.0, 1 / 9.0 } };
            Convolution(kernel, this.image);
        }
        /// <summary>
        /// Cette méthode fait appel à la méthode conculution en appliquant un kernel qui effectue un repousage
        /// </summary>
        public void embossing()
        {
            // repoussage
            double[,] kernel = new double[,] { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
            Convolution(kernel, this.image);
        }
        /// <summary>
        /// Cette méthode fait appel à la méthode conculution en appliquant un kernel qui effectue un renforcement des bords
        /// </summary>
        public void edge_reforming()
        {
            //renforcement des bords 
            double[,] kernel = new double[,] { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };
            Convolution(kernel, this.image);
        }
        /// <summary>
        /// Cette méthode fait appel à la méthode conculution en appliquant un kernel qui augmente le contraste
        /// </summary>
        public void increase_contrast()
        {
            //augmentation du contraste/ netteté
            //int[,] kernel = new int[,] { { 0, 0, 0, 0, 0 }, { 0, 0, -1, 0, 0 }, { 0, -1, 5, -1, 0 }, { 0, 0, -1, 0, 0 }, { 0, 0, 0, 0, 0 } };
            double[,] kernel = new double[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
            Convolution(kernel, this.image);
        }

        /// <summary>
        /// Cette fonction permet de cacher l'image pris en argument au sein de l'image prise en argument
        /// </summary>
        /// <param name="hidenimage"></param>
        public void hideimage(MyImage hidenimage) {

            // Pour l'image dans laquelle on cache 
            string[,] tabimageR = new string[this.image.GetLength(0), this.image.GetLength(1)];
            string[,] tabimageG = new string[this.image.GetLength(0), this.image.GetLength(1)];
            string[,] tabimageB = new string[this.image.GetLength(0), this.image.GetLength(1)];

            string[,] tabhidenR = new string[this.image.GetLength(0), this.image.GetLength(1)];
            string[,] tabhidenG = new string[this.image.GetLength(0), this.image.GetLength(1)];
            string[,] tabhidenB = new string[this.image.GetLength(0), this.image.GetLength(1)];
            int l = this.image.GetLength(0);
            int c = this.image.GetLength(1);

            tabimageR = DectabToBintab(this.image, 0, l, c);
            tabimageG = DectabToBintab(this.image, 1, l, c);
            tabimageB = DectabToBintab(this.image, 2, l, c);

            tabhidenR = DectabToBintab(hidenimage.image, 0, l, c);
            tabhidenG = DectabToBintab(hidenimage.image, 1, l, c);
            tabhidenB = DectabToBintab(hidenimage.image, 2, l, c);

            refill(tabhidenR);
            refill(tabhidenG);
            refill(tabhidenB);

            cut(tabimageR);
            cut(tabimageG);
            cut(tabimageB);

            cut(tabhidenR);
            cut(tabhidenG);
            cut(tabhidenB);

            string[,] R = concatenate(tabimageR, tabhidenR);
            string[,] G = concatenate(tabimageG, tabhidenG);
            string[,] B = concatenate(tabimageB, tabhidenB);

            To8bits(R);
            To8bits(G);
            To8bits(B);

            this.image = BinToDec(R, G, B);
        }

        /// <summary>
        /// 0=rouge; 1=vert; 2=bleu
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private string[,] DectabToBintab(RGB[,] image, int color, int l, int c) {
            string[,] tabres = new string[l, c];
            if (color == 0) {
                for (int i = 0; i < image.GetLength(0); i++) {
                    for (int j = 0; j < image.GetLength(1); j++) {
                        tabres[i, j] = decToBinary(image[i, j].R);
                    }
                }
            }
            if (color == 1) {
                for (int i = 0; i < image.GetLength(0); i++) {
                    for (int j = 0; j < image.GetLength(1); j++) {
                        tabres[i, j] = decToBinary(image[i, j].G);
                    }
                }
            }
            if (color == 2) {
                for (int i = 0; i < image.GetLength(0); i++) {
                    for (int j = 0; j < image.GetLength(1); j++) {
                        tabres[i, j] = decToBinary(image[i, j].B);
                    }
                }
            }

            return tabres;
        }

        /// <summary>
        /// Cette fonction permet de supprimer les 4 derniers bits de l'octet présent dans la chaine de caractère
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private string[,] cut(string[,] tab) {
            string[,] tabres = new string[tab.GetLength(0), tab.GetLength(1)];
            for (int i = 0; i < tab.GetLength(0); i++) {
                for (int j = 0; j < tab.GetLength(1); j++) {
                    tabres[i, j] = tab[i, j].Substring(4, 4);
                    //Console.WriteLine(tabres[i, j]);
                }
            }

            return tabres;
        }

        /// <summary>
        /// Cette fonction transforme un entier en base 2 en un octet
        /// </summary>
        /// <param name="n"></param>
        /// <param name="bitNumber"></param>
        /// <returns></returns>
        public string decToBinary(int n, int bitNumber = 8) {

            string s = "";

            for (int i = bitNumber - 1; i >= 0; i--) {
                int k = n >> i;
                if ((k & 1) > 0)
                    s += "1";
                else
                    s += "0";
            }
            return s;
        }

        /// <summary>
        /// cette fonction permet de concaténer les chaines de caractères de deux tableaux différents avec l'élément correpondant
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private string[,] concatenate(string[,] a, string[,] b) {
            string[,] tabres = new string[a.GetLength(0), a.GetLength(1)];
            for (int i = 0; i < tabres.GetLength(0); i++) {
                for (int j = 0; j < tabres.GetLength(1); j++) {
                    tabres[i, j] = b[i, j] + a[i, j];
                }
            }
            return tabres;
        }

        /// <summary>
        /// Cette fonction permet de rajouter les 0 manquants afin de former un octet
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private string[,] To8bits(string[,] tab) {
            for (int i = 0; i < tab.GetLength(0); i++) {
                for (int j = 0; j < tab.GetLength(1); j++) {
                    tab[i, j] = tab[i, j].PadRight(8, '0');
                }
            }
            return tab;
        }

        /// <summary>
        /// cette fonction transforme 3 tableaux de strings un pour chaque couleur en matrice RGB
        /// </summary>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private RGB[,] BinToDec(string[,] R, string[,] G, string[,] B) {
            RGB[,] res = new RGB[R.GetLength(0), R.GetLength(1)];
            for (int i = 0; i < R.GetLength(0); i++) {
                for (int j = 0; j < R.GetLength(1); j++) {

                    res[i, j] = RGB.FromRGB((byte)Convert.ToInt32(R[i, j], 2), (byte)Convert.ToInt32(G[i, j], 2), (byte)Convert.ToInt32(B[i, j], 2));
                    //Console.WriteLine(res[i, j].R);
                }
            }

            return res;
        }

        /// <summary>
        /// cette fonction permet de remplir en tableau de string avec "00000000"
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private string[,] refill(string[,] tab) {
            string[,] tabres = new string[tab.GetLength(0), tab.GetLength(1)];
            for (int i = 0; i < tabres.GetLength(0); i++) {
                for (int j = 0; j < tabres.GetLength(1); j++) {
                    if (tab[i, j] == null) {
                        tab[i, j] = "00000000";
                    }
                }
            }
            return tabres;
        }

        /// <summary>
        /// cette fonction permet de décoder l'image cachée
        /// </summary>
        /// <returns></returns>
        public RGB[,] unhideimage() {
            RGB[,] res = new RGB[this.image.GetLength(0), this.image.GetLength(1)];

            string[,] tabimageR = new string[this.image.GetLength(0), this.image.GetLength(1)];
            string[,] tabimageG = new string[this.image.GetLength(0), this.image.GetLength(1)];
            string[,] tabimageB = new string[this.image.GetLength(0), this.image.GetLength(1)];

            int l = this.image.GetLength(0);
            int c = this.image.GetLength(1);

            tabimageR = DectabToBintab(this.image, 0, l, c);
            tabimageG = DectabToBintab(this.image, 1, l, c);
            tabimageB = DectabToBintab(this.image, 2, l, c);

            cut2(tabimageR);
            cut2(tabimageG);
            cut2(tabimageB);

            To8bits(tabimageR);
            To8bits(tabimageG);
            To8bits(tabimageB);

            res = BinToDec(tabimageR, tabimageG, tabimageB);

            return res;
        }
        /// <summary>
        /// cette fonction enlève les 4 premiers bits
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private string[,] cut2(string[,] tab) {
            string[,] tabres = new string[tab.GetLength(0), tab.GetLength(1)];
            for (int i = 0; i < tab.GetLength(0); i++) {
                for (int j = 0; j < tab.GetLength(1); j++) {
                    tabres[i, j] = tab[i, j].Substring(0, 4);
                    //Console.WriteLine(tabres[i, j]);
                }
            }

            return tabres;
        }

        /// <summary>
        /// Ce constructeur permet de créer une image à partir d'un RGB[,]
        /// </summary>
        /// <param name="im"></param>
        public MyImage(RGB[,] im) {

            this.image = im;

            this.type = "BM";
            byte[] b = { 230, 4, 0, 0 };
            this.fileSize = LEToInt(b);
            this.offsetSize = 54;
            this.width = image.GetLength(1);
            this.height = image.GetLength(0);
            this.size = width * height * 3 + 54;
            this.bitsPerPixel = 24;
            this.compression = intToLE(0);
            this.imageSize = intToLE(0);
            this.XpixelsPerM = intToLE(0);
            this.YpixelsPerM = intToLE(0);
            this.ColorsUsed = intToLE(0);
            this.ImportantColors = intToLE(0);

            rotation90();
            rotation90();

        }
    }
}


