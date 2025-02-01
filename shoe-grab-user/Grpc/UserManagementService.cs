using Grpc.Core;
using AutoMapper;
using ShoeGrabCommonModels.Contexts;

namespace ShoeGrabUserManagement.Grpc;

public class UserManagementService : UserManagement.UserManagementBase
{
    private readonly UserContext _context;
    private readonly IMapper _mapper;

    public UserManagementService(UserContext context, IMapper mapper)
    {
        _context = context; 
        _mapper = mapper;
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var user = await _context.Users.FindAsync(request.Id);
        return new GetUserResponse
        {
            User = _mapper.Map<UserProto>(user),
        };
    }
}
