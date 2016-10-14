using AutoMapper;
using AzureDataAccess.Clients;
using WebAuth.Models.Profile;

namespace WebAuth.Configurations
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<PersonalDataEntity, PersonalInformationViewModel>();
            CreateMap<PersonalDataEntity, CountryOfResidenceViewModel>();
            CreateMap<PersonalDataEntity, AddressInformationViewModel>();
        }
    }
}