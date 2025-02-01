using AutoMapper;
using ShoeGrabCommonModels;

namespace ShoeGrabOrderManagement.Database.Mappers;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        CreateMap<User, UserProto>();
    }
}
