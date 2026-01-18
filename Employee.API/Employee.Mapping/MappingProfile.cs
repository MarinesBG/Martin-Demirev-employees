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

            CreateMap<PairCalculationResult, ResultViewModel>()
                .ForMember(dest => dest.TopPair,
                          opt => opt.MapFrom(src => src.TopPair))
                .ForMember(dest => dest.AllPairs,
                          opt => opt.MapFrom(src => src.AllPairs))
                .ForMember(dest => dest.TotalPairsFound,
                          opt => opt.MapFrom(src => src.AllPairs.Count))
                .ForMember(dest => dest.Message,
                          opt => opt.MapFrom(src =>
                              src.TopPair != null
                                  ? $"Top pair: Employees {src.TopPair.EmployeeIdA} and {src.TopPair.EmployeeIdB} worked together for {src.TopPair.TotalDays} days"
                                  : "No pairs found"));
        }
    }
}
