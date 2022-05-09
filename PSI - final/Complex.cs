using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI {
    class Complex {
        private double Re;
        private double Im;
        /// <summary>
        /// Constructeur pour Complex
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        public Complex(double Re, double Im) {
            this.Re = Re;
            this.Im = Im;
        }

        /// <summary>
        /// Calcule le module d'un complexe
        /// </summary>
        /// <param name="z"></param>
        /// <returns>
        /// Le module d'un complexe en double
        /// </returns>
        public static double Modulus(Complex z) {
            return Math.Pow(Math.Pow(z.Re, 2) + Math.Pow(z.Im, 2), 0.5);
        }
        
        /// <summary>
        /// Redéfinition de l'opérateur + pour fonctionner avec les complexes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a, Complex b) {
            return new Complex(a.Re + b.Re, a.Im + b.Im);
        }

        /// <summary>
        /// Redéfinition d'un opérateur * pour fonctionner avec les complexes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, Complex b) {
            return new Complex(a.Re*b.Re - a.Im*b.Im, a.Im*b.Re + a.Re*b.Im);
        }

        public double R {
            get { return Re; }
        }
        public double I {
            get { return Im; }
        }

        public override string ToString() {
            return Re + " " + Im;
        }
    }
}
