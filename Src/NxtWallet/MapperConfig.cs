using AutoMapper;
using NxtLib;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;
using Transaction = NxtWallet.ViewModel.Model.Transaction;

namespace NxtWallet
{
    public class MapperConfig
    {
        private static MapperConfiguration _configuration;

        public static MapperConfiguration Setup(IWalletRepository repo)
        {
            if (_configuration != null)
                return _configuration;

            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ContactDto, Contact>();
                cfg.CreateMap<Contact, ContactDto>();
                cfg.CreateMap<TransactionDto, Transaction>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => (ulong)src.NxtId))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => (TransactionSubType)src.TransactionType))
                    .AfterMap((src, dest) => dest.UserIsRecipient = repo.NxtAccount.AccountRs.Equals(dest.AccountTo))
                    .AfterMap((src, dest) => dest.UserIsSender = repo.NxtAccount.AccountRs.Equals(dest.AccountFrom));

                cfg.CreateMap<Transaction, TransactionDto>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => (long)src.NxtId))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => (int)src.TransactionType));

                cfg.CreateMap<NxtLib.Transaction, Transaction>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => src.TransactionId ?? 0))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => GetMessage(src)))
                    .ForMember(dest => dest.NqtAmount, opt => opt.MapFrom(src => src.Amount.Nqt))
                    .ForMember(dest => dest.NqtFee, opt => opt.MapFrom(src => src.Fee.Nqt))
                    .ForMember(dest => dest.AccountFrom, opt => opt.MapFrom(src => src.SenderRs))
                    .ForMember(dest => dest.AccountTo, opt => opt.MapFrom(src => src.RecipientRs))
                    .ForMember(dest => dest.IsConfirmed, opt => opt.MapFrom(src => src.Confirmations != null))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.SubType))
                    .AfterMap((src, dest) => dest.UserIsRecipient = repo.NxtAccount.AccountRs.Equals(dest.AccountTo))
                    .AfterMap((src, dest) => dest.UserIsSender = repo.NxtAccount.AccountRs.Equals(dest.AccountFrom));
            });

            return _configuration;
        }

        private static string GetMessage(NxtLib.Transaction transaction)
        {
            if (transaction.SubType == TransactionSubType.PaymentOrdinaryPayment ||
                transaction.SubType == TransactionSubType.MessagingArbitraryMessage)
            {
                return transaction.Message?.MessageText;
            }
            return transaction.SubType.ToString();
        }
    }
}
