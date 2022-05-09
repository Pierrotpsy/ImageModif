using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI {
    public class RGB {
        private byte r;
        private byte g;
        private byte b;

        /// <summary>
        /// Constructeur pour RGB
        /// </summary>
        public RGB() {
            this.r = 255;
            this.g = 255;
            this.b = 255;
        }

        public byte R {
            get { return r; }
            set { r = value; }
        }
        public byte G {
            get { return g; }
            set { g = value; }
        }
        public byte B {
            get { return b; }
            set { b = value; }
        }

        /// <summary>
        /// Cette méthode crée un RGB et lui assigne les valeurs entrées en paramètre
        /// </summary>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static RGB FromRGB(byte R, byte G, byte B) {
            RGB c = new RGB();
            c.r = R;
            c.b = B;
            c.g = G;
            return c;
        }
        /// <summary>
        /// Cette méthode retourne un string donnant la valeur de chaque couleur
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return R + "|" + G + "|" + B;
        }
    }
}
