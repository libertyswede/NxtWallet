using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NxtWallet.Core.Fakes;
using NxtWallet.Core;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Diagnostics;
using AutoMapper;

namespace NxtWallet.IntegrationTest
{
    [TestClass]
    public class BackgroundRunnerTest
    {
        private List<Transaction> _addedTransactions = new List<Transaction>();
        private static IMapper _mapper = MapperConfig.Setup().CreateMapper();

        [TestMethod]
        public async Task TryCheckAllTransactionsForAllTestnetAccountsTest()
        {
            await TryCheckAllTransactionsTest("NXT-G885-AKDX-5G2B-BLUCG");
            //await TryCheckAllTransactionsTest("NXT-7A48-47JL-T7LD-D5FS3");
        }

        [TestMethod]
        public async Task TryCheckAllTransactionsForAllTestnetAccountsTest2()
        {
            //await TryCheckAllTransactionsTest("NXT-G885-AKDX-5G2B-BLUCG");
            await TryCheckAllTransactionsTest("NXT-7A48-47JL-T7LD-D5FS3");
        }

        public async Task TryCheckAllTransactionsTest(string accountRs)
        {
            var walletRepository = new FakeWalletRepository();
            walletRepository.NxtAccount = accountRs;
            walletRepository.IsReadOnlyAccount = true;
            walletRepository.Balance = "0";
            walletRepository.SecretPhrase = string.Empty;
            walletRepository.LastLedgerEntryBlockId = 2680262203532249785UL;
            walletRepository.NxtServer = "http://localhost:6876/nxt";
            walletRepository.SleepTime = 10000;
            walletRepository.BackupCompleted = true;
            walletRepository.NotificationsEnabled = false;
            walletRepository.LastAssetTrade = new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            walletRepository.LastCurrencyExchange = new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);

            var serviceFactory = new NxtLib.ServiceFactory(walletRepository.NxtServer);
            var nxtServer = new NxtServer(walletRepository, _mapper, serviceFactory);
            var transactionRepository = new FakeTransactionRepository(walletRepository);
            var balanceCalculator = new BalanceCalculator();
            var contactRepository = new FakeContactRepository();
            var assetRepository = new FakeAssetRepository();
            var assetTracker = new AssetTracker(assetRepository, serviceFactory, _mapper, balanceCalculator);
            var msCurrencyTracker = new MsCurrencyTracker(nxtServer, walletRepository);

            var runner = new BackgroundRunner(nxtServer, transactionRepository, balanceCalculator,
                walletRepository, contactRepository, assetTracker, msCurrencyTracker);

            _addedTransactions.Clear();
            runner.TransactionAdded += (sender, transaction) => _addedTransactions.Add(transaction);

            await runner.TryCheckAllTransactions();

            if (_addedTransactions.Any() && runner.UnconfirmedBalanceNqt != _addedTransactions.OrderBy(t => t.Timestamp).Last().NqtBalance)
            {
                var filePath = await CheckAddedTransactions();
                throw new Exception("Balances dont match! Log file here: " + filePath);
            }
        }

        private async Task<string> CheckAddedTransactions()
        {
            var orderedTransactions = _addedTransactions.OrderByDescending(t => t.Timestamp).ToList();

            var storageFolder = ApplicationData.Current.LocalFolder;
            var sampleFile = await storageFolder.CreateFileAsync("added_transactions.txt", CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine("Outputfile: " + sampleFile.Path);

            using (var stream = await sampleFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var outputStream = stream.GetOutputStreamAt(0))
            using (var dataWriter = new DataWriter(outputStream))
            {
                var header = "Balance".PadLeft(15) + " - " +
                    "Amount".PadLeft(15) + " - " +
                    "Fee".PadLeft(15) + " - " +
                    "TransactionId".PadLeft(20) + " - " +
                    "Transaction Type".PadLeft(30) + " - " +
                    "Timestamp".PadLeft(19) + " - " +
                    "Height".PadLeft(10) + "\r\n";

                Debug.Write(header);
                dataWriter.WriteString(header);

                foreach (var transaction in orderedTransactions)
                {
                    var formattedTransaction = transaction.FormattedBalance.PadLeft(15) + " - " +
                        transaction.FormattedAmount.PadLeft(15) + " - " +
                        transaction.FormattedFee.PadLeft(15) + " - " +
                        transaction.NxtId.ToString().PadLeft(20) + " - " +
                        transaction.TransactionType.ToString().PadLeft(30) + " - " +
                        transaction.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + " - " +
                        transaction.Height.ToString().PadLeft(10) + "\r\n";

                    Debug.Write(formattedTransaction);
                    dataWriter.WriteString(formattedTransaction);
                }

                await dataWriter.StoreAsync();
                await outputStream.FlushAsync();
            }

            return sampleFile.Path;
        }
    }
}
