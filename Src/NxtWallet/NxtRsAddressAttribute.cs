using NxtLib;
using System;
using System.ComponentModel.DataAnnotations;

namespace NxtWallet
{
    public class NxtRsAddressAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            try
            {
                var account = new Account(value.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
