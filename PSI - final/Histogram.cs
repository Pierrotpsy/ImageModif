using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI
{
    public class Histogram
    {
        private int[,] tabcolor;
        public RGB[,] graph;

        /// <summary>
        /// Ce constructeur répertorie l'intensité de chaque couleur dans une matrice
        /// La matrice se décompose de la manière suivante : la première ligne les valeurs pour le rouge sur la seconde la valeur pour le vert et la troisième est dernière le bleu.
        /// La martice a une taille de 3*256
        /// Ce constructeur appelle également la fonction qui permet de créer une matrice RGB représentant le graphique.
        /// </summary>
        /// <param name="image"></param>
        public Histogram(MyImage image)
        {
            this.tabcolor = new int[3, 256]; // 0 pour le rouge; 1 pour le vert; 2 pour le bleu

            for (int i = 0; i < image.image.GetLength(0); i++)
            {
                for (int j = 0; j < image.image.GetLength(1); j++)
                {

                    this.tabcolor[0, image.image[i, j].R]++;
                    this.tabcolor[1, image.image[i, j].G]++;
                    this.tabcolor[2, image.image[i, j].B]++;
                }
            }

            graphhisto(this.tabcolor);


        }

        /// <summary>
        /// Cette fobnction permet de créer une matrice RGB représenant les trois histogramme de chaque couleur
        /// </summary>
        /// <param name="tab"></param>
        public void graphhisto(int[,] tab)
        {
            this.graph = new RGB[600, 256];
            RGB rouge, bleu, vert, noir;
            rouge = RGB.FromRGB(255, 0, 0);
            vert = RGB.FromRGB(0, 255, 0);
            bleu = RGB.FromRGB(0, 0, 255);
            noir = RGB.FromRGB(0, 0, 0);
            int max;

            max = recherchemax(tab);

            int ordonne;
            for (int abscisse=0;abscisse<256;abscisse++)
            {
                for (int index=0; index<3;index++)
                {
                    ordonne = position(200, max, tab[index, abscisse]);
                    for (int k=200-1; k>200-1-ordonne;k--)
                    {
                        if (index==0)
                        {
                            graph[k , abscisse] = rouge;
                        }
                        if(index==1)
                        {
                            graph[k + ( 200), abscisse] = vert;
                        }
                        if(index==2)
                        {
                            graph[k + ( 400), abscisse] = bleu;
                        }
                    }
                }
            }

            for(int i=0; i<600;i++)
            {
                for(int j=0; j<256;j++)
                {
                    if (graph[i,j]==null)
                    {
                        graph[i, j] = noir;
                    }
                }
            }
            //graph[0, 0] = RGB.FromRGB(255, 255, 255);
        }


        /// <summary>
        /// cette fonction permet de trouver la position du "pic" d'intensité en fonction de la hauteur du plus grand pic
        /// Elle fait un produit en croix
        /// </summary>
        /// <param name="hauteur"></param>
        /// <param name="max"></param>
        /// <param name="valeur"></param>
        /// <returns>
        /// elle retourne la position
        /// </returns>
        public int position(int hauteur, int max, int valeur)
        {
            
            int res = 0;
            res = (valeur * hauteur) / max;
            return res;
        }

        /// <summary>
        /// Cette fonction permet d'obtenir le maximum d'intensité des trois couleurs afin de créer un graphique à la bonne échelle.
        /// </summary>
        /// <param name="tab"></param>
        /// <returns>
        /// elle retourne la valeur maximale
        /// </returns>
        public int recherchemax(int[,] tab)
        {
            int res = 0;
            for(int i=0;i<tab.GetLength(0);i++)
            {
                for(int j=0; j<tab.GetLength(1);j++)
                {
                    if (res < tab[i, j])
                    {
                        res = tab[i, j];
                    }
 
                }
            }
            return res;
        }
    }
}
