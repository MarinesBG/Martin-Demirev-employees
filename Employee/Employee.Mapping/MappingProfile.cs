using AutoMapper;
using Employee.Contracts.Models;
using Employee.Domain.Models;

namespace Employee.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PairResult, PairResultViewModel>()
                .ForMember(dest => dest.Projects,
                          opt => opt.MapFrom(src => src.Projects));

            CreateMap<ProjectDetail, PairProjectViewModel>();
        }
    }
}
