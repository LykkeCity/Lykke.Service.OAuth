using AutoMapper;
using Core.Clients;
using Lykke.Service.PersonalData.Contract.Models;
using WebAuth.Models.Profile;

namespace WebAuth.Configurations
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<IPersonalData, PersonalInformationViewModel>();
            CreateMap<IPersonalData, AddressInformationViewModel>();
        }
    }
}
