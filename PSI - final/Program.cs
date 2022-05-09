using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI {
    public class Program {
        static void Main(string[] args) {
            

            QRcode qr= new QRcode("https://twiki.org/cgi-bin/view/Blog/BlogEntry201102x2", 'L');
            MyImage i = new MyImage(qr);
            i.Resize(8);
            i.MyImageToFile("qr.bmp");
            Process.Start("qr.bmp");

            Console.ReadKey();
        }
    }
}
