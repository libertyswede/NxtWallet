using AutoMapper;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;

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
                cfg.CreateMap<Contact, ContactModel>();
                cfg.CreateMap<ContactModel, Contact>();
                cfg.CreateMap<Transaction, TransactionModel>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => (ulong)src.NxtId))
                    .AfterMap((src, dest) => dest.UserIsRecipient = repo.NxtAccount.AccountRs.Equals(dest.AccountTo));

                cfg.CreateMap<TransactionModel, Transaction>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => (long)src.NxtId));

                cfg.CreateMap<NxtLib.Transaction, TransactionModel>()
                    .ForMember(dest => dest.NxtId, opt => opt.MapFrom(src => src.TransactionId ?? 0))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message != null ? src.Message.MessageText : null))
                    .ForMember(dest => dest.NqtAmount, opt => opt.MapFrom(src => src.Amount.Nqt))
                    .ForMember(dest => dest.NqtFee, opt => opt.MapFrom(src => src.Fee.Nqt))
                    .ForMember(dest => dest.AccountFrom, opt => opt.MapFrom(src => src.SenderRs))
                    .ForMember(dest => dest.AccountTo, opt => opt.MapFrom(src => src.RecipientRs))
                    .ForMember(dest => dest.IsConfirmed, opt => opt.MapFrom(src => src.Confirmations.HasValue && src.Confirmations.Value > 0))
                    .AfterMap((src, dest) => dest.UserIsRecipient = repo.NxtAccount.AccountRs.Equals(dest.AccountTo));
            });

            return _configuration;
        }
    }
}
