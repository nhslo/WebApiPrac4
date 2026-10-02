using AutoMapper;
using WebApiPrac4.DTOs;
using WebApiPrac4.Models;

namespace WebApiPrac4.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Team, TeamDto>().ReverseMap();
        CreateMap<TeamInputDto, Team>();
    }
}
