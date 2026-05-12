using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTO;
using WebApplication1.Model;

namespace WebApplication1.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerRequest.Email))
            throw new InvalidOperationException("User already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = registerRequest.FullName,
            Email = registerRequest.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password)
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        bool isValid = BCrypt.Net.BCrypt.Verify(
            loginRequest.Password,
            user.PasswordHash
        );

        if (!isValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        return GenerateAuthResponse(user);
    }

    private AuthResponseDto GenerateAuthResponse(User user)
    {
        return new AuthResponseDto
        {
            Token = "test-token",
            FullName = user.FullName,
            Email = user.Email
        };
    }
}