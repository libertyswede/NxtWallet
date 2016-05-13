using Microsoft.VisualStudio.TestTools.UnitTesting;
using NxtWallet.Model;

namespace NxtWallet.Test
{
    [TestClass]
    public class MsCurrencyTrackerTest
    {
        MsCurrencyTracker tracker;

        [TestInitialize]
        public void MsCurrencyTrackerTestInitialize()
        {
            tracker = new MsCurrencyTracker(new FakeNxtServer(), new FakeWalletRepository());
        }

        [TestMethod]
        public void MyTestMethod()
        {
            
        }
    }
}
