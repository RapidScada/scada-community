using CustomScripts;
using Scada.Data.Entities;
using Scada.Data.Models;

namespace CustomScriptsTest
{
    /// <summary>
    /// Tests scripts.
    /// </summary>
    [TestClass]
    public sealed class MyScriptsTest
    {
        [TestMethod]
        public void LinearTransformTest()
        {
            MyScripts myScripts = new();
            myScripts.BeginCalcCnlData(101, new Cnl(), new CnlData(100.0, 1));
            double actualResult = myScripts.LinearTransform(5, 10);
            Assert.AreEqual(510, actualResult);
        }

        [TestMethod]
        public void CnlValSumTest()
        {
            MyScripts myScripts = new();
            myScripts.SetData(101, 1.0, 1);
            myScripts.SetData(102, 2.0, 1);
            myScripts.SetData(103, 3.0, 1);
            myScripts.SetData(104, 4.0, 1);
            myScripts.SetData(105, 5.0, 1);
            double actualResult = myScripts.CnlValSum(101, 102, 103, 104, 105);
            Assert.AreEqual(15, actualResult);
        }
    }
}
