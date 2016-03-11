using System;

namespace NxtWallet.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public long NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public string Account { get; set; }
        public string Message { get; set; }

        public ulong GetTransactionId()
        {
            return (ulong) NxtId;
        }
    }
}