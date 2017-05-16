using AutoMapper;
using Core.Clients;
using WebAuth.Models.Profile;

namespace WebAuth.Configurations
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<IPersonalData, PersonalInformationViewModel>();
            CreateMap<IPersonalData, CountryOfResidenceViewModel>();
            CreateMap<IPersonalData, AddressInformationViewModel>();
        }
    }
}