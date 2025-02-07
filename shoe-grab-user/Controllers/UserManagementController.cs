using Microsoft.AspNetCore.Mvc;
using ShoeGrabUserManagement.Models.Dto;
using ShoeGrabCommonModels.Contexts;
using Microsoft.EntityFrameworkCore;
using ShoeGrabUserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using User = ShoeGrabCommonModels.User;
using ShoeGrabCommonModels;

namespace ShoeGrabUserManagement.Controllers;
[ApiController]
[Route("api/auth")]
public class UserManagementController : ControllerBase
{
    private readonly UserContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordManagement _passwordManagement;

    public UserManagementController(UserContext context, ITokenService tokenService, IPasswordManagement passwordManagement)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordManagement = passwordManagement;
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
    public async Task<IActionResult> GetProfile()
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

        return Ok(new
        {
            profile.User.Username,
            profile.User.Email,
            profile.Address,
            profile.PhoneNumber,
            profile.DateOfBirth,
            profile.Bio
        });
    }

    [HttpGet("profile/update")]
    [Authorize]
    public async Task<IActionResult> EditProfile([FromQuery]EditProfileDto profileDto)
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

        userProfile.Address = profileDto.Address;
        userProfile.PhoneNumber = profileDto.PhoneNumber;
        userProfile.Bio = profileDto.Bio;
        userProfile.DateOfBirth = profileDto.DateOfBirth;

        _context.Profiles.Update(userProfile);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Profile updated successfully." });
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
}
