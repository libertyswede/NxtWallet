﻿using AutoMapper;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NxtWallet.Core;
using NxtWallet.Core.Fakes;
using NxtWallet.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxtWallet.IntegrationTest
{
    [TestClass]
    public class AccountLedgerRunnerTest
    {
        private List<LedgerEntry> _addedLedgerEntries = new List<LedgerEntry>();
        private static IMapper _mapper = MapperConfig.Setup().CreateMapper();

        [TestMethod]
        public async Task TryCheckAllTransactionsForAllTestnetAccountsTest()
        {
            await TryCheckAllTransactionsTest("NXT-G885-AKDX-5G2B-BLUCG");
            //await TryCheckAllTransactionsTest("NXT-7A48-47JL-T7LD-D5FS3");
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

            var serviceFactory = new NxtLib.ServiceFactory(walletRepository.NxtServer);
            var nxtServer = new NxtServer(walletRepository, _mapper, serviceFactory);
            var transactionRepository = new FakeAccountLedgerRepository();
            var contactRepository = new FakeContactRepository();

            var runner = new AccountLedgerRunner(walletRepository, nxtServer);

            _addedLedgerEntries.Clear();
            runner.AccountLedgerAdded += (sender, ledgerEntry) => _addedLedgerEntries.Add(ledgerEntry);

            await runner.TryCheckAllTransactions();

            var previousTimestamp = DateTime.MinValue;
            for (int i = _addedLedgerEntries.Count - 1; i >= 0; i--)
            {
                var addedLedgerEntry = _addedLedgerEntries[i];
                if (addedLedgerEntry.Timestamp < previousTimestamp)
                {
                    throw new Exception("Error!");
                }
                previousTimestamp = addedLedgerEntry.Timestamp;
            }


            var previousBalance = 0L;
            for (int i = _addedLedgerEntries.Count - 1; i >= 0; i--)
            {
                var addedLedgerEntry = _addedLedgerEntries[i];
                var calculatedBalance = previousBalance;
                calculatedBalance += addedLedgerEntry.NqtAmount;
                calculatedBalance += (addedLedgerEntry.UserIsTransactionSender) ? addedLedgerEntry.NqtFee : 0;
                if (addedLedgerEntry.NqtBalance != calculatedBalance)
                {
                    throw new Exception("Error!");
                }
                previousBalance = calculatedBalance;
            }
        }
    }
}
