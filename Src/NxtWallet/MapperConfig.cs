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
            });

            return _configuration;
        }
    }
}
