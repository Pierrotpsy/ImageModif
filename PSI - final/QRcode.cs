using ReedSolomon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI {
    public class QRcode {
        private string data;
        private struct ECC {
            public int dataCodewordsTotal;
            public int blocksG1;
            public int eccPerBlock;
            public int dataCodewordsG1;
            public int blocksG2;
            public int dataCodewordsG2;

            public ECC(int dataCodewordsTotal, int eccPerBlock, int blocksG1, int dataCodewordsG1, int blocksG2, int dataCodewordsG2) {
                this.dataCodewordsTotal = dataCodewordsTotal;
                this.blocksG1 = blocksG1;
                this.eccPerBlock = eccPerBlock;
                this.dataCodewordsG1 = dataCodewordsG1;
                this.blocksG2 = blocksG2;
                this.dataCodewordsG2 = dataCodewordsG2;
            }
        }
        private string modeIndicator = "0010";
        private string charCountIndicator;
        private string encodedData = "";
        private string finalData;
        private char errorCorrect;
        private int version;
        private int mask;
        private int brea;
        private int[,] finderPattern = { {1, 1, 1, 1, 1, 1, 1},
                                         {1, 0, 0, 0, 0, 0, 1},
                                         {1, 0, 1, 1, 1, 0, 1},
                                         {1, 0, 1, 1, 1, 0, 1},
                                         {1, 0, 1, 1, 1, 0, 1},
                                         {1, 0, 0, 0, 0, 0, 1},
                                         {1, 1, 1, 1, 1, 1, 1}
                                       };
        private int[,] alignmentPattern = { {1, 1, 1, 1, 1},
                                             {1, 0, 0, 0, 1},
                                             {1, 0, 1, 0, 1},
                                             {1, 0, 0, 0, 1},
                                             {1, 1, 1, 1, 1}
                                           };
        private Dictionary<char, int> alphanumeric = new Dictionary<char, int>();
        private Dictionary<string, ECC> errorCorrection = new Dictionary<string, ECC>();
        private Dictionary<string, int> characterCapacity = new Dictionary<string, int>();
        private Dictionary<int, int[]> alignmentPositions = new Dictionary<int, int[]>();
        public RGB[,] image;
        private RGB WPattern = RGB.FromRGB(128, 0, 128);
        private RGB BPattern = RGB.FromRGB(0, 128, 0);
        private RGB W = RGB.FromRGB(255, 255, 255);
        private RGB B = RGB.FromRGB(0, 0, 0);
        private RGB R = RGB.FromRGB(255, 0, 0);
        private List<byte[]> dataBytes1 = new List<byte[]>();
        private List<byte[]> results1 = new List<byte[]>();
        private List<byte[]> dataBytes2 = new List<byte[]>();
        private List<byte[]> results2 = new List<byte[]>();

        /// <summary>
        /// Constructeur pour le QRcode
        /// </summary>
        /// <param name="data"></param>
        /// <param name="errorCorrect"></param>
        public QRcode(string data, char errorCorrect) {
            this.data = data.ToUpper();
            this.errorCorrect = errorCorrect;

            FillcharacterCapacityDictionary("characterCapacity.txt");
            FillAplhanumericDictionary("Alphanumeric.txt");
            FillerrorCorrectionDictionary("errorCorrection.txt");
            FillAlignmentPositions("alignmentPositions.txt");

            if (chooseSize()) {
                image = new RGB[21+(version-1)*4,21+(version-1)*4];
            } else throw new SystemException("Chaine de caractères trop longue");
            brea = (errorCorrection[version + "-" + errorCorrect].dataCodewordsTotal + errorCorrection[version + "-" + errorCorrect].eccPerBlock) * 8*8;
            begin();
        }

        /// <summary>
        /// Fonction pour gérer le bon déroulement de la création du QR code.
        /// </summary>
        private void begin() {
            int nbits = 0;
            if (version >= 27) {
                nbits = 13;
            } else if (version < 27 && version > 9) {
                nbits = 11;
            } else nbits = 9;
            charCountIndicator = decToBinary(data.Length, nbits);
            encodeData();


            finalData = modeIndicator + charCountIndicator + encodedData;
            int terminator = errorCorrection[version + "-" + errorCorrect].dataCodewordsTotal*8 - finalData.Length;

            if (terminator < 4) {
                for (int i = 0; i < terminator; i++) finalData += "0";
            } else finalData += "0000";


            int multipleEight = 8 - finalData.Length % 8;

            for (int i = 0; i < multipleEight; i++) finalData += "0";
            int padbytes = (errorCorrection[version + "-" + errorCorrect].dataCodewordsTotal*8 - finalData.Length)/8;

            bool invert = true;
            for (int i = 0; i < padbytes; i++) {
                if (invert) {
                    finalData += decToBinary(236, 8);
                    invert = false;
                } else {
                    finalData += decToBinary(17, 8);
                    invert = true;
                }
            }

            getECC();

            interleave();

            placePatterns();

            fillQRCode();
           
            masking();

            fillInfo();

            masking(true);
            int a = 0;
            for (int i = 0; i < image.GetLength(0); i++) {
                for (int j = 0; j < image.GetLength(0); j++) {
                    if (image[i, j] == null) image[i, j] = RGB.FromRGB(0, 255, 0);
                    a++;
                }
            }

        }

        /// <summary>
        /// Fonction pour bien distribuer les bits de données et les bits du code de correction d'erreurs.
        /// </summary>
        private void interleave() {
            finalData = "";
            int dataint = Math.Max(errorCorrection[version + "-" + errorCorrect].dataCodewordsG1, errorCorrection[version + "-" + errorCorrect].dataCodewordsG2);

            for(int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG1; i++) {
                for(int j = 0; j < dataint ; j++) {
                    if(j < dataBytes1[i].Length) finalData += Convert.ToString(dataBytes1[i][j], 2).PadLeft(8, '0');
                }
            }
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG2; i++) {
                for (int j = 0; j < dataint; j++) {
                    if (j < dataBytes2[i].Length) finalData += Convert.ToString(dataBytes2[i][j], 2).PadLeft(8, '0');
                }
            }
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG1; i++) {
                for (int j = 0; j < errorCorrection[version + "-" + errorCorrect].eccPerBlock; j++) {
                    if (j < results1[i].Length) finalData += Convert.ToString(results1[i][j], 2).PadLeft(8, '0');
                }
            }
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG2; i++) {
                for (int j = 0; j < errorCorrection[version + "-" + errorCorrect].eccPerBlock; j++) {
                    if (j < results2[i].Length) finalData += Convert.ToString(results2[i][j], 2).PadLeft(8, '0');
                }
            }

            if (version == 2 || version == 3 || version == 4 || version == 5 || version == 6) {
                for(int h = 0; h < 7; h++) {
                    finalData += "0";
                    brea++;
                }
            } else if (version == 14 || version == 15 || version == 16 || version == 17 || version == 18 || version == 19 || version == 20 || version == 28 || version == 29 || version == 30 || version == 31 || version == 32 || version == 33 || version == 34) {
                for (int h = 0; h < 3; h++) {
                    finalData += "0";
                    brea++;
                }
            } else if (version == 21 || version == 22 || version == 23 || version == 24 || version == 25 || version == 26 || version == 27) {
                for (int h = 0; h < 4; h++) {
                    finalData += "0";
                    brea++;
                }
            }
        }

        /// <summary>
        /// Fonction pour obtenir les bytes de correction d'erreurs
        /// </summary>
        private void getECC() {
            for(int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG1; i++) {
                dataBytes1.Add(new byte[errorCorrection[version + "-" + errorCorrect].dataCodewordsG1]);
                results1.Add(new byte[errorCorrection[version + "-" + errorCorrect].eccPerBlock]);
            }
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG2; i++) {
                dataBytes2.Add(new byte[errorCorrection[version + "-" + errorCorrect].dataCodewordsG2]);
                results2.Add(new byte[errorCorrection[version + "-" + errorCorrect].eccPerBlock]);
            }

            int offset = 8;
            int offset_groups = 0;
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG1; i++) {
                for(int j = 0; j < dataBytes1[i].Length; j++) {
                    int startIndex = offset_groups + finalData.Length - 8 * (j+1);
                    if (startIndex < 0) {
                        offset = 8 + startIndex;
                        startIndex = 0;
                    }
                    dataBytes1[i][dataBytes1[i].Length - j - 1] = Convert.ToByte(finalData.Substring(startIndex, offset), 2);
                }
                results1[i] = ReedSolomonAlgorithm.Encode(dataBytes1[i], errorCorrection[version + "-" + errorCorrect].eccPerBlock, ErrorCorrectionCodeType.QRCode);
                offset_groups -= 8 * dataBytes1[i].Length;
            }

            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG2; i++) {
                for (int j = 0; j < dataBytes2[i].Length; j++) {
                    int startIndex = offset_groups + finalData.Length - 8 * (j+1);
                    if (startIndex < 0) {
                        offset = 8 + startIndex;
                        startIndex = 0;
                    }
                    dataBytes2[i][dataBytes2[i].Length - j - 1] = Convert.ToByte(finalData.Substring(startIndex, offset), 2);
                }
                results2[i] = ReedSolomonAlgorithm.Encode(dataBytes2[i], errorCorrection[version + "-" + errorCorrect].eccPerBlock, ErrorCorrectionCodeType.QRCode);
                offset_groups -= 8 * dataBytes2[i].Length;
            }
            //Le code suivant est gardé en cas de debuggage
            /*
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG1; i++) {
                Console.WriteLine("\nbyteData1_" + i);
                for (int j = 0; j < dataBytes1[i].Length; j++) {
                    Console.Write(dataBytes1[i][j] + " ");
                }
                Console.WriteLine("\nresult1_" + i);
                for (int j = 0; j < results1[i].Length; j++) {
                    Console.Write(results1[i][j] + " ");
                }
            }
            for (int i = 0; i < errorCorrection[version + "-" + errorCorrect].blocksG2; i++) {
                Console.WriteLine("\nbyteData2_" + i);
                for (int j = 0; j < dataBytes2[i].Length; j++) {
                    Console.Write(dataBytes2[i][j] + " ");
                }
                Console.WriteLine("\nresult2_" + i);
                for (int j = 0; j < results2[i].Length; j++) {
                    Console.Write(results2[i][j] + " ");
                }
            }*/
        }

        /// <summary>
        /// Fonction pour déterminer la taille du QR code à créer en fonction du nombre de caractères entrés.
        /// </summary>
        /// <returns>
        /// True si le nombre de caractères est pris en charge par le format alphanumérique.
        /// </returns>
        private bool chooseSize() {
            int a = 1;
            while(a < 160 && data.Length > characterCapacity[a.ToString() + errorCorrect.ToString()]) {
                a++;
            }
            version = a;
            return true;

        }


        /// <summary>
        /// Fonction remplit le dictionnaire contenant l'équivalence caractères/nombres de la norme alphanumérique à partir d'un fichier.
        /// </summary>
        /// <param name="path"></param>
        private void FillAplhanumericDictionary(string path) {
            try {
                StreamReader reader = new StreamReader(path);
                string[] lines = new string[File.ReadAllLines(path).Length];
                int i = 0;
                while (reader.Peek() > 0) {
                    lines[i] = reader.ReadLine();
                    i++;
                }

                for(int h = 0; h < 45; h++) {
                alphanumeric.Add((char)lines[h][0], int.Parse(lines[h].Substring(2, 2)));
                }
                reader.Close();
            } catch {
                throw new System.Exception("Erreur de lecture. Vérifier le chemin.");
            }

        }
        /// <summary>
        /// Fonction remplit le dictionnaire contenant les informations des blocs de données et de correction d'erreur à partir d'un fichier.
        /// </summary>
        /// <param name="path"></param>
        private void FillerrorCorrectionDictionary(string path) {
            try {
                StreamReader reader = new StreamReader(path);
                string[] lines = new string[File.ReadAllLines(path).Length];
                int i = 0;
                while (reader.Peek() > 0) {
                    lines[i] = reader.ReadLine();
                    i++;
                }
                string[] data;
                char sep = ';';
                for(int h = 0; h < lines.Length; h++) {
                    data = lines[h].Split(sep);
                    int t1 = data[5] != "" ? int.Parse(data[5]) : 0;
                    int t2 = data[5] != "" ? int.Parse(data[6]) : 0;

                    errorCorrection.Add(data[0], new ECC(int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), t1, t2));

                }
                reader.Close();
            } catch {
                throw new System.Exception("Erreur de lecture. Vérifier le chemin.");
            }
        }

        /// <summary>
        /// Fonction remplit le dictionnaire contenant les capacités de caractères maximales par version et correction de QR code à partir d'un fichier.
        /// </summary>
        /// <param name="path"></param>
        private void FillcharacterCapacityDictionary(string path) {
            try {
                StreamReader reader = new StreamReader(path);
                string[] lines = new string[File.ReadAllLines(path).Length];
                int i = 0;
                while (reader.Peek() > 0) {
                    lines[i] = reader.ReadLine();
                    i++;
                }

                for (int h = 0; h < lines.Length; h++) {
                    characterCapacity.Add(lines[h].Substring(0, 3).Replace(" ", ""), int.Parse(lines[h].Substring(3, lines[h].Length - 4)));
                }
                reader.Close();
            } catch {
                throw new System.Exception("Erreur de lecture. Vérifier le chemin.");
            }
        }

        /// <summary>
        /// Fonction remplit le dictionnaire contenant les coordonnées des pattern d'alignement à partir d'un fichier.
        /// </summary>
        /// <param name="path"></param>
        private void FillAlignmentPositions(string path) {
            try {
                StreamReader reader = new StreamReader(path);
                string[] lines = new string[File.ReadAllLines(path).Length];
                int i = 0;
                while (reader.Peek() > 0) {
                    lines[i] = reader.ReadLine();
                    i++;
                }
                string[] data;
                char sep = ';';
                for (int h = 0; h < lines.Length; h++) {
                    data = lines[h].Split(sep);
                    alignmentPositions.Add(int.Parse(data[0]), new int[7]);

                    alignmentPositions[int.Parse(data[0])][0] = data[1] != "" ? int.Parse(data[1]) : 0;
                    alignmentPositions[int.Parse(data[0])][1] = data[2] != "" ? int.Parse(data[2]) : 0;
                    alignmentPositions[int.Parse(data[0])][2] = data[3] != "" ? int.Parse(data[3]) : 0;
                    alignmentPositions[int.Parse(data[0])][3] = data[4] != "" ? int.Parse(data[4]) : 0;
                    alignmentPositions[int.Parse(data[0])][4] = data[5] != "" ? int.Parse(data[5]) : 0;
                    alignmentPositions[int.Parse(data[0])][5] = data[6] != "" ? int.Parse(data[6]) : 0;
                    alignmentPositions[int.Parse(data[0])][6] = data[7] != "" ? int.Parse(data[7]) : 0;
                }
                reader.Close();
            } catch {
                throw new System.Exception("Erreur de lecture. Vérifier le chemin.");
            }
        }

        /// <summary>
        /// Fonction qui permet de convertir le string à encoder dans le QR code en bits suivant la norme alphanumérique
        /// </summary>
        private void encodeData() {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Length; i++) {
                if (i != 0 && i % 2 == 0) sb.Append('|');
                sb.Append(data[i]);
            }
            string[] formatted = sb.ToString().Split('|');

            for (int i = 0; i < formatted.Length; i++) {
                if (formatted[i].Length == 2) {
                    encodedData += decToBinary(45 * alphanumeric[formatted[i][0]] + alphanumeric[formatted[i][1]], 11);
                } else {
                    encodedData += decToBinary(alphanumeric[formatted[i][0]], 6);
                }
            }
        }

        /// <summary>
        /// Fonction qui permet de convertir un entier en binaire
        /// </summary>
        /// <param name="n"></param>
        /// <param name="bitNumber"></param>
        /// <returns>
        /// Une chaîne de caractères qui correspond au paramètre d'entrée en binaire
        /// </returns>
        private string decToBinary(int n, int bitNumber = 8) {

            string s = "";

            for (int i = bitNumber-1; i >= 0; i--) {
                int k = n >> i;
                if ((k & 1) > 0)
                    s += "1";
                else
                    s+= "0";
            }
            return s;
        }

        /// <summary>
        /// Fonction qui gère l'ensemble des pattern du QR code selon les versions
        /// </summary>
        private void placePatterns() {
            placeFinderPattern(0, 0);
            placeFinderPattern(((version - 1) * 4) + 21 - 7, 0);
            placeFinderPattern(0, ((version - 1) * 4) + 21 - 7);

            placeSeparators();

            if(version >=2) {
                for (int i = 0; i < alignmentPositions[version].Length; i++) {
                    for (int j = 0; j < alignmentPositions[version].Length; j++) {
                        if(alignmentPositions[version][i] != 0 && alignmentPositions[version][j] != 0) placeAlignmentPatterns(alignmentPositions[version][i], alignmentPositions[version][j]);
                    }
                }
            }

            placeTimingPatterns();
            
            reserveInformationAreas();

            image[8, 4 * version + 9] = BPattern;
        }

        /// <summary>
        /// Fonction qui place des pattern pour respecter la norme du QR code
        /// </summary>
        private void placeFinderPattern(int x, int y) {
            for(int i = 0; i < 7; i++) {
                for(int j = 0; j < 7; j++) {
                    image[x + i, y + j] = finderPattern[i, j] == 0 ? WPattern : BPattern; 
                }
            }
        }

        /// <summary>
        /// Fonction qui place des pattern pour respecter la norme du QR code
        /// </summary>
        private void placeSeparators() {
            int l = image.GetLength(0);
            for(int i = 0; i < l; i++) {
                _= image[i, 6] != null ? image[i, 7] = WPattern : image[i,7] = null;
            }
            for (int i = 0; i < l; i++) {
                _ = image[i, l - 7] != null ? image[i, l-8] = WPattern : image[i, l-8] = null;
            }
            for (int i = 0; i < l; i++) {
                _ = image[6, i] != null? image[7, i] = WPattern : image[7, i] = null;
            }
            for (int i = 0; i < l; i++) {
                _ = image[l - 7, i] != null ? image[l - 8, i] = WPattern : image[l - 8, i] = null;
            }
        }
        /// <summary>
        /// Fonction qui place des pattern pour respecter la norme du QR code
        /// </summary>
        private void placeAlignmentPatterns(int x, int y) {
            bool check = true;
            for(int i = -2; i < 3; i++) {
                for(int j = -2; j < 3; j++) {
                    if (image[x + i, y + j] != null) check = false;
                }
            }

            if(check) {
                int x_start = x - 2;
                int y_start = y - 2;
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 5; j++) {
                        image[x_start + i, y_start + j] = alignmentPattern[i, j] == 0 ? WPattern : BPattern;
                    }
                }
            }
        }

        /// <summary>
        /// Fonction qui place des pattern pour respecter la norme du QR code
        /// </summary>
        private void placeTimingPatterns() {
            int l = image.GetLength(0);
            bool invertR = true;
            bool invertC = true;
            for (int i = 0; i < l; i++) {
                if (image[i, 6] == null) {
                    if(invertC) {
                        image[i, 6] = BPattern;
                        invertC = false;
                    } else {
                        image[i, 6] = WPattern;
                        invertC = true;
                    }
                }
                if (image[6, i] == null) {
                    if (invertR) {
                        image[6, i] = BPattern;
                        invertR = false;
                    } else {
                        image[6, i] = WPattern;
                        invertR = true;
                    }
                }
            }
        }

        /// <summary>
        /// Fonction qui colore les bits d'information sur le QR code pour les préserver lors du remplissage.
        /// </summary>
        private void reserveInformationAreas() {
            int l = image.GetLength(0);
            for (int i = 0; i < l; i++) {
                _ = image[i, 1] != null && image[i,8] == null? image[i, 8] = R : null;

                _ = image[7, i] != null && image[8, i] == null ? image[8, i] = R : null;
            }

            if(version > 6) {
                int x = image.GetLength(0) - 11;
                int y = 0;
                for (int i = 0; i < 6; i++) {
                    for (int j = 0; j < 3; j++) {
                        image[x + j, y + i] = RGB.FromRGB(0, 0, 255);
                        image[y + i, x + j] = RGB.FromRGB(0, 0, 255);
                    }
                }
            }
        }

        /// <summary>
        /// Fonction qui remplit le QR code bit par bit.
        /// </summary>
        private void fillQRCode() {
            int l = image.GetLength(0);
            int a = 0;
            int y = l - 1;
            bool invert = false;
            while(y >0) {
                if (!invert) {
                    for (int i = 0; i < l; i++) {
                        if (image[y,l - 1 - i] == null && a <brea) {
                            image[y,l - 1 - i] = int.Parse(finalData[a].ToString()) == 0 ? W : B;
                            a++;
                        }
                        if (image[y - 1,l - 1 - i] == null && a < brea) {
                            image[y-1, l - 1 - i] = int.Parse(finalData[a].ToString()) == 0 ? W : B;
                            a++;
                        }
                        invert = true;
                    }
                } else if (invert) {
                    for (int i = 0; i < l; i++) {
                        if (image[y,i] == null && a < brea) {
                            image[y,i] = int.Parse(finalData[a].ToString()) == 0 ? W : B;
                            a++;
                        }
                        if (image[y - 1,i] == null && a < brea) {

                            image[y - 1, i] = int.Parse(finalData[a].ToString()) == 0 ? W : B;
                            a++;
                        }
                        invert = false;
                    }
                }
                if (y == 8) y--;
                y -= 2;

            }
        }

        /// <summary>
        /// Fonction qui choisit et appplique le masque approprié au QR code généré.
        /// </summary>
        /// <param name="final"></param>
        private void masking(bool final = false) {
            int l = image.GetLength(0);
            Dictionary<int, RGB[,]> masks= new Dictionary<int, RGB[,]>();
            for(int i = 0; i < 8; i++) {
                masks.Add(i, new RGB[l, l]);
            }
            for (int i = 0; i < l; i++) {
                for (int j = 0; j < l; j++) {
                    for (int k = 0; k < 8; k++) {
                        masks[k][i, j] = image[i, j];
                    }
                }
            }

            int[] pen = new int[8];

            for (int i = 0; i < l; i++) {
                for (int j = 0; j < l; j++) {
                    if ((i + j) % 2 == 0 && image[j, i] == B) {
                        masks[0][j, i] = W;
                    } else if((i + j) % 2 == 0 && image[j, i] == W) masks[0][j, i] = B;

                    if (i % 2 == 0 && image[j, i] == B) {
                        masks[1][j, i] = W;
                    } else if (i % 2 == 0 && image[j, i] == W) masks[1][j, i] = B;

                    if (j % 3 == 0 && image[j, i] == B) {
                        masks[2][j, i] = W;
                    } else if (j % 3 == 0 && image[j, i] == W) masks[2][j, i] = B;

                    if ((i + j) % 3 == 0 && image[j, i] == B) {
                        masks[3][j, i] = W;
                    } else if ((i + j) % 3 == 0 && image[j, i] == W) masks[3][j, i] = B;

                    if ((Math.Floor(j / 2.0) + Math.Floor(i / 3.0)) % 2 == 0 && image[i, j] == B) {
                        masks[4][i, j] = W;
                    } else if ((Math.Floor(j / 2.0) + Math.Floor(i / 3.0)) % 2 == 0 && image[i, j] == W) masks[4][i, j] = B;

                    if (((i * j) % 2) + ((i*j) % 3) == 0 && image[j, i] == B) {
                        masks[5][j, i] = W;
                    } else if (((i * j) % 2) + ((i * j) % 3) == 0 && image[j, i] == W) masks[5][j, i] = B;

                    if ((((i * j) % 2) + ((i * j) % 3)) % 2 == 0 && image[j, i] == B) {
                        masks[6][j, i] = W;
                    } else if ((((i * j) % 2) + ((i * j) % 3)) % 2 == 0 && image[j, i] == W) masks[6][j, i] = B;

                    if ((((i + j) % 2) + ((i * j) % 3)) % 2 == 0 && image[j, i] == B) {
                        masks[7][j, i] = W;
                    } else if ((((i + j) % 2) + ((i * j) % 3)) % 2 == 0 && image[j, i] == W) masks[7][j, i] = B;
                }
            }

            

            for (int i = 0; i < 8; i++) {
                pen[i] = penalty(masks[i]);
            }

            int pos = 0;
            for (int i = 0; i < pen.Length; i++) {
                if (pen[i] < pen[pos]) { pos = i; }
            }
            //pos = 0;
            mask = pos;


            for (int i = 0; i < l; i++) {
                for (int j = 0; j < l; j++) {
                    if (masks[pos][i, j] == WPattern) {
                        masks[pos][i, j] = W;
                    } else if (masks[pos][i, j] == BPattern) masks[pos][i, j] = B;
                    
                }
            }
            if (final) image = masks[pos];
        }

        /// <summary>
        /// Fonction pour calculer les 4 pénalités liées au masquage
        /// </summary>
        /// <param name="image"></param>
        /// <returns>
        /// Le score de l'image entrée en paramètre
        /// </returns>
        private int penalty(RGB[,] image) {
            int l = image.GetLength(0);
            int score1 = 0;
            int score2 = 0;
            int score3 = 0;
            int score4 = 0;
            int wCount = 0;
            int bCount = 0;
            int wCountTotal = 0;
            int bCountTotal = 0;

            RGB[] pattern1 = { B, W, B, B, B, W, B, W, W, W, W };
            RGB[] pattern2 = { W, W, W, W, B, W, B, B, B, W, B };
            for (int i = 0; i < l; i++) {
                for (int j = 0; j < l; j++) {
                    if (image[i, j] == W) {
                        if (bCount >= 5) {
                            bCount -= 5;
                            score1 += 3;
                            while (bCount > 0) {
                                bCount--;
                                score1++;
                            }
                        }
                        bCount = 0;
                        wCountTotal++;
                        wCount++;
                    } else {
                        if (wCount >= 5) {
                            wCount -= 5;
                            score1 += 3;
                            while (wCount > 0) {
                                wCount--;
                                score1++;
                            }
                        }
                        wCount = 0;
                        bCountTotal++;
                        bCount++;
                    }
                }
            }

            for (int j = 0; j < l; j++) {
                for (int i = 0; i < l; i++) {
                    if (image[i, j] == W) {
                        if (bCount >= 5) {
                            bCount -= 5;
                            score1 += 3;
                            while (bCount > 0) {
                                bCount--;
                                score1++;
                            }
                        }
                        bCount = 0;
                        wCount++;
                    } else {
                        if (wCount >= 5) {
                            wCount -= 5;
                            score1 += 3;
                            while (wCount > 0) {
                                wCount--;
                                score1++;
                            }
                        }
                        wCount = 0;
                        bCount++;
                    }
                }
            }

            for (int i = 0; i < l; i++) {
                for (int j = 0; j < l; j++) {
                    if ((i + 1 < l && j + 1 < l) && image[i, j] == W && image[i + 1, j] == W && image[i, j + 1] == W && image[i + 1, j + 1] == W) {
                        score2 += 3;
                    } else if ((i + 1 < l && j + 1 < l) && image[i, j] == B && image[i + 1, j] == B && image[i, j + 1] == B && image[i + 1, j + 1] == B) {
                        score2 += 3;
                    }
                }
            }
            int n1 = 0;
            int n2 = 0;
            for (int i = 0; i < l; i++) {
                for (int j = 0; j < l; j++) {
                    if (i + 11 < l) {
                        while (n1 < 11 && image[i + n1, j] == pattern1[n1]) {
                            n1++;
                        }

                        while (n2 < 11 && image[i + n2, j] == pattern2[n2]) {
                            n2++;
                        }

                        if (n1 == 10 || n2 == 10) {
                            n1 = 0;
                            n2 = 0;
                            score3 += 40;
                        }
                    }
                    if (j + 11 < l) {
                        while (n1 < 11 && image[i, j + n1] == pattern1[n1]) {
                            n1++;
                        }

                        while (n2 < 11 && image[i, j + n2] == pattern2[n2]) {
                            n2++;
                        }

                        if (n1 == 10 || n2 == 10) {
                            n1 = 0;
                            n2 = 0;
                            score3 += 40;
                        }
                    }
                }
            }

            int pen4 = 100 * bCountTotal / (bCountTotal + wCountTotal);
            int nextPen4 = 2*pen4 - 5*(pen4 % 5);
            int previousPen4 = pen4 - pen4 % 5;
            nextPen4 = Math.Abs(nextPen4 - 50)/5;
            previousPen4 = Math.Abs(previousPen4 - 50)/5;
            score4 += nextPen4 < previousPen4 ? previousPen4 * 10 : nextPen4 * 10;
            return score1+score2+score3+score4;
        }

        /// <summary>
        /// Fonction qui remplit les bits d'info du QR code.
        /// </summary>
        private void fillInfo() {
            string format = "01" + decToBinary(mask, 3);
            format = format.PadRight(15, '0');
            format = format.TrimStart('0');
            string generator = "10100110111";
            string result = "";
            int l = format.Length;
            while(l >= 11) {
                generator = generator.PadRight(l, '0');
                result = "";
                for (int i = 0; i < l; i++) {
                    result += (format[i] ^ generator[i]).ToString();
                }
                result= result.TrimStart('0');
                format = result;
                l = result.Length;
                generator = "10100110111";
            }
            if (l < 10) result = result.PadLeft(10, '0');
            format = "01" + decToBinary(mask, 3);
            string results = format + result;

            string final = "";
            string binaryS = "101010000010010";
            for (int i = 0; i < 15; i++) {
                final += results[i] ^ binaryS[i];
            }
            image[0, 8] = int.Parse(final[0].ToString()) == 1 ? BPattern : WPattern;
            image[1, 8] = int.Parse(final[1].ToString()) == 1 ? BPattern : WPattern;
            image[2, 8] = int.Parse(final[2].ToString()) == 1 ? BPattern : WPattern;
            image[3, 8] = int.Parse(final[3].ToString()) == 1 ? BPattern : WPattern;
            image[4, 8] = int.Parse(final[4].ToString()) == 1 ? BPattern : WPattern;
            image[5, 8] = int.Parse(final[5].ToString()) == 1 ? BPattern : WPattern;
            image[7, 8] = int.Parse(final[6].ToString()) == 1 ? BPattern : WPattern;
            image[8, 8] = int.Parse(final[7].ToString()) == 1 ? BPattern : WPattern;
            image[8, 7] = int.Parse(final[8].ToString()) == 1 ? BPattern : WPattern;
            image[8, 5] = int.Parse(final[9].ToString()) == 1 ? BPattern : WPattern;
            image[8, 4] = int.Parse(final[10].ToString()) == 1 ? BPattern : WPattern;
            image[8, 3] = int.Parse(final[11].ToString()) == 1 ? BPattern : WPattern;
            image[8, 2] = int.Parse(final[12].ToString()) == 1 ? BPattern : WPattern;
            image[8, 1] = int.Parse(final[13].ToString()) == 1 ? BPattern : WPattern;
            image[8, 0] = int.Parse(final[14].ToString()) == 1 ? BPattern : WPattern;
            
            image[8, image.GetLength(0)-1] = int.Parse(final[0].ToString()) == 1 ? BPattern : WPattern;
            image[8, image.GetLength(0) - 2] = int.Parse(final[1].ToString()) == 1 ? BPattern : WPattern;
            image[8,image.GetLength(0) - 3] = int.Parse(final[2].ToString()) == 1 ? BPattern : WPattern;
            image[8,image.GetLength(0) - 4] = int.Parse(final[3].ToString()) == 1 ? BPattern : WPattern;
            image[8, image.GetLength(0) - 5] = int.Parse(final[4].ToString()) == 1 ? BPattern : WPattern;
            image[8,image.GetLength(0) - 6] = int.Parse(final[5].ToString()) == 1 ? BPattern : WPattern;
            image[8,image.GetLength(0) - 7] = int.Parse(final[6].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 8,8] = int.Parse(final[7].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 7,8] = int.Parse(final[8].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 6,8] = int.Parse(final[9].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 5,8] = int.Parse(final[10].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 4,8] = int.Parse(final[11].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 3, 8] = int.Parse(final[12].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 2, 8] = int.Parse(final[13].ToString()) == 1 ? BPattern : WPattern;
            image[image.GetLength(0) - 1, 8] = int.Parse(final[14].ToString()) == 1 ? BPattern : WPattern;
        }
    }
}
