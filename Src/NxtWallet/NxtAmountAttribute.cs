using NxtLib;
using System;
using System.ComponentModel.DataAnnotations;

namespace NxtWallet
{
    public class NxtAmountAttribute : ValidationAttribute
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }

        public override bool IsValid(object value)
        {
            try
            {
                var result = decimal.Parse(value.ToString());
                var amount = Amount.CreateAmountFromNxt(result);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
