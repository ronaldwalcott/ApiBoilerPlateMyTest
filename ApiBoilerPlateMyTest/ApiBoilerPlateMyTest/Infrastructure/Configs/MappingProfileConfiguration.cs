using AutoMapper;
using ApiBoilerPlateMyTest.Data.Entity;
using ApiBoilerPlateMyTest.DTO;
using ApiBoilerPlateMyTest.DTO.Response;
using ApiBoilerPlateMyTest.DTO.Request;

namespace ApiBoilerPlateMyTest.Infrastructure.Configs
{
    public class MappingProfileConfiguration : Profile
    {
        public MappingProfileConfiguration()
        {
            CreateMap<Person, CreatePersonRequest>().ReverseMap();
            CreateMap<Person, UpdatePersonRequest>().ReverseMap();
            CreateMap<Person, PersonResponse>().ReverseMap();
        }
    }
}
