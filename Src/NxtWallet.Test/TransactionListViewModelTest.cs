using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NxtWallet.Model;
using NxtWallet.ViewModel;

namespace NxtWallet.Test
{
    [TestClass]
    public class TransactionListViewModelTest
    {
        private TransactionListViewModel _viewmodel;

        [TestInitialize]
        public void TestInitialize()
        {
            _viewmodel = new TransactionListViewModel(new FakeWalletRepository(), new FakeBackgroundRunner());
        }

        [TestMethod]
        public void LoadTransactionsFromRepositoryShouldAddTransactions()
        {
            _viewmodel.LoadTransactionsFromRepository();

            Assert.AreEqual(3, _viewmodel.Transactions.Count);
        }
    }
}
