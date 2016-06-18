using System;

namespace NxtWallet.Core.ViewModel.Model
{
    public class MsUndoCrowdfundingTransaction : Transaction, IEquatable<MsUndoCrowdfundingTransaction>
    {
        public long ReserveIncreaseNxtId { get; set; }


        public override bool Equals(object obj)
        {
            var transaction = obj as MsUndoCrowdfundingTransaction;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ ReserveIncreaseNxtId.GetHashCode();
            }
        }

        public bool Equals(MsUndoCrowdfundingTransaction other)
        {
            return other?.ReserveIncreaseNxtId == ReserveIncreaseNxtId;
        }
    }
}