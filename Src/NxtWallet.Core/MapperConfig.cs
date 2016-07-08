using System.Text.RegularExpressions;
using AutoMapper;
using NxtLib;
using NxtWallet.Core.Models;
using NxtWallet.Core.Migrations.Model;

namespace NxtWallet.Core
{
    public class MapperConfig
    {
        private static MapperConfiguration _configuration;

        public static MapperConfiguration Setup()
        {
            if (_configuration != null)
                return _configuration;

            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ContactDto, Contact>();
                cfg.CreateMap<Contact, ContactDto>();
                cfg.CreateMap<LedgerEntryDto, LedgerEntry>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => (ulong?)src.TransactionId))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => (LedgerEntryType)src.TransactionType));

                cfg.CreateMap<LedgerEntry, LedgerEntryDto>()
                    .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => (long?)src.NxtId))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => (int)src.TransactionType));

                cfg.CreateMap<Transaction, LedgerEntry>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => src.TransactionId ?? 0))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => GetMessage(src)))
                    .ForMember(dest => dest.NqtAmount, opt => opt.MapFrom(src => src.Amount.Nqt))
                    .ForMember(dest => dest.NqtFee, opt => opt.MapFrom(src => src.Fee.Nqt))
                    .ForMember(dest => dest.AccountFrom, opt => opt.MapFrom(src => src.SenderRs))
                    .ForMember(dest => dest.AccountTo, opt => opt.MapFrom(src => src.RecipientRs))
                    .ForMember(dest => dest.IsConfirmed, opt => opt.MapFrom(src => src.Confirmations != null))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => (LedgerEntryType)(int)src.SubType));
            });

            return _configuration;
        }

        private static string GetMessage(Transaction transaction)
        {
            if (transaction.SubType == TransactionSubType.PaymentOrdinaryPayment ||
                transaction.SubType == TransactionSubType.MessagingArbitraryMessage)
            {
                return transaction.Message?.MessageText;
            }
            var input = "[" + (LedgerEntryType)(int)transaction.SubType + "]";
            return Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }
    }
}
