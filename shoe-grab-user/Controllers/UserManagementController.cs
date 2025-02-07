using Microsoft.AspNetCore.Mvc;
using ShoeGrabUserManagement.Models.Dto;
using ShoeGrabCommonModels.Contexts;
using Microsoft.EntityFrameworkCore;
using ShoeGrabUserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using User = ShoeGrabCommonModels.User;
using ShoeGrabCommonModels;
using AutoMapper;

namespace ShoeGrabUserManagement.Controllers;
[ApiController]
[Route("api/auth")]
public class UserManagementController : ControllerBase
{
    private readonly UserContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordManagement _passwordManagement;
    private readonly IMapper _mapper;

    public UserManagementController(UserContext context, ITokenService tokenService, IPasswordManagement passwordManagement, IMapper mapper)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordManagement = passwordManagement;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
        {
            return Conflict(new { Message = "Email already in use." });
        }

        if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
        {
            return Conflict(new { Message = "Username already in use." });
        }

        var hashedPassword = _passwordManagement.HashPassword(userDto.Password);

        var user = new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
        if (user == null || !_passwordManagement.VerifyPassword(userDto.Password, user.PasswordHash))
        {
            return NotFound(new { Message = "Invalid email or password." });
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new { Message = "Login successful.", Token = token });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileWithUserDataDto>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.Authentication)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var profile = await _context.Profiles
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.UserId == int.Parse(userId));

        if (profile == null)
        {
            return NotFound(new { Message = "User profile not found." });
        }

        var mappedProfile = _mapper.Map<UserProfileDto>(profile);
        var response = new UserProfileWithUserDataDto
        {
            Username = profile.User.Username,
            Email = profile.User.Email,
            Profile = mappedProfile
        };
        return Ok(response);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> EditProfile(UserProfileDto profileDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.Authentication)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var userProfile = await _context.Profiles.FirstOrDefaultAsync(up => up.UserId == int.Parse(userId));

        if (userProfile == null)
        {
            return NotFound(new { Message = "User profile not found." });
        }

        var mappedProfile = _mapper.Map<UserProfile>(profileDto);
        
        userProfile.PhoneNumber = mappedProfile.PhoneNumber;
        userProfile.Address = mappedProfile.Address;
        userProfile.DateOfBirth = mappedProfile.DateOfBirth;
        userProfile.Bio = mappedProfile.Bio;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true });
        }
        catch (Exception)
        {
            return Ok(new { Success = false });
        }
    }

    [HttpGet("role")]
    [Authorize]
    public async Task<ActionResult<string>> GetUserRole()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == null)
        {
            return BadRequest();
        }

        return await Task.FromResult(Ok(new { userRole }));
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(EditPasswordDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.Authentication)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null) 
        {
            return NotFound("User not found");
        }
        var correctOldPassword = _passwordManagement.HashPassword(request.OldPassword) == user.PasswordHash;

        if (!correctOldPassword)
        {
            return Unauthorized("Wrong password!");
        }

        user.PasswordHash = _passwordManagement.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync();

        return Ok();
    }
}
