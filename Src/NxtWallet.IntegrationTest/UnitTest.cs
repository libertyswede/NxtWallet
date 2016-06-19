using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NxtWallet.Core.Fakes;
using NxtWallet.Core;
using System.Threading.Tasks;

namespace NxtWallet.IntegrationTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var walletRepository = new FakeWalletRepository();
            walletRepository.NxtAccount = "NXT-G885-AKDX-5G2B-BLUCG";
            walletRepository.IsReadOnlyAccount = true;
            walletRepository.Balance = "0";
            walletRepository.SecretPhrase = "abc123";
            walletRepository.LastBalanceMatchBlockId = 2680262203532249785UL;
            walletRepository.NxtServer = "http://localhost:6876/nxt";
            walletRepository.SleepTime = 10000;
            walletRepository.BackupCompleted = true;
            walletRepository.NotificationsEnabled = false;
            walletRepository.LastAssetTrade = new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            walletRepository.LastCurrencyExchange = new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);

            var mapper = MapperConfig.Setup(walletRepository).CreateMapper();
            var serviceFactory = new NxtLib.ServiceFactory(walletRepository.NxtServer);
            var nxtServer = new NxtServer(walletRepository, mapper, serviceFactory);
            var transactionRepository = new FakeTransactionRepository(walletRepository);
            var balanceCalculator = new BalanceCalculator();
            var contactRepository = new FakeContactRepository();
            var assetRepository = new FakeAssetRepository();
            var assetTracker = new AssetTracker(assetRepository, serviceFactory, mapper, balanceCalculator);
            var msCurrencyTracker = new MsCurrencyTracker(nxtServer, walletRepository);

            var runner = new BackgroundRunner(nxtServer, transactionRepository, balanceCalculator,
                walletRepository, contactRepository, assetTracker, msCurrencyTracker);

            await runner.TryCheckAllTransactions();
        }
    }
}
