using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSI;
namespace PSI_Test {
    [TestClass]
    public class MyImageTest {

        [TestMethod]
        public void TestLEToInt() {
            byte[] t = { 220, 155, 76, 32 };
            MyImage i = new MyImage();
            int result = 191;
            int test = i.LEToInt(t);
            Assert.AreEqual(result, test, "La fonction ne produit pas le bon résultat");
        }
        public void TestIntToLE() {
            int t = 91;
            MyImage i = new MyImage();
            byte result = 220;
            byte[] test = i.intToLE(t);
            Assert.AreEqual(result, test, "La fonction ne produit pas le bon résultat");
        }

        [TestMethod]
        public void TestDecToBinary() {
            int t = 90;
            MyImage i = new MyImage();
            string result = "01011010";
            string test = i.decToBinary(t);
            Assert.AreEqual(result, test, "La fonction ne produit pas le bon résultat");

        }

        
    }
}
