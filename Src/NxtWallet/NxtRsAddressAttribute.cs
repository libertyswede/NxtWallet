using NxtLib;
using NxtWallet.Core.Models;
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
                var address = Contact.GetAddressOrInput(value.ToString());
                var account = new Account(address);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
