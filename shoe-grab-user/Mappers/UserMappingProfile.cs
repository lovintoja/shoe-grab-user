using AutoMapper;
using ShoeGrabCommonModels;
using ShoeGrabUserManagement.Models.Dto;

namespace ShoeGrabUserManagement.Mappers;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<UserProfileDto, UserProfile>();
        CreateMap<AddressDto, Address>();
        CreateMap<UserProfile, UserProfileDto>();
        CreateMap<Address, AddressDto>();

    }
}
