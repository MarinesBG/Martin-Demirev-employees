using AutoMapper;
using Employee.Contracts.Models;
using Employee.Domain.Models;

namespace Employee.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PairResult, PairResultViewModel>();
        }
    }
}
