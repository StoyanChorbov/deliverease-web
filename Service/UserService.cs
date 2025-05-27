using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Model;
using Model.DTO.User;
using Repository;

namespace Service;

public class UserService(
    UserRepository userRepository,
    TokenRepository tokenRepository,
    UserManager<User> userManager,
    IConfiguration configuration)
{
    // Get userDto by username
    public async Task<UserDto> GetAsync(string username)
    {
        return ToDto(await GetUserByUsernameAsync(username));
    }

    // Get user entity by username
    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var normalizedUsername = userManager.NormalizeName(username);
        var user = await userManager.FindByNameAsync(normalizedUsername);

        if (user == null)
            throw new Exception("User not found");

        return user;
    }

    // Get all users by usernames
    public async Task<List<User>> GetAllByUsernamesAsync(List<string> usernames)
    {
        return await userRepository.GetAllByUsernamesAsync(
            usernames: usernames,
            useNavigationalProperties: false,
            isReadOnly: false
        );
    }

    public async Task<User?> GetByJwtTokenAsync(string token)
    {
        return await userRepository.GetByJwtTokenAsync(token);
    }

    // Login a user and return the JWT token and refresh token
    public async Task<Tuple<string?, string?>?> LoginAsync(UserLoginDto userLoginDto)
    {
        var password = userLoginDto.Password;
        var user = await userManager.FindByNameAsync(userLoginDto.Username);

        if (user == null || !await userManager.CheckPasswordAsync(user, password)) return null;

        var generatedAuthToken = await GetAuthToken(user);
        var authToken = new JwtSecurityTokenHandler().WriteToken(generatedAuthToken);
        var refreshToken = GenerateRefreshToken();

        var token = new JwtToken
        {
            Token = authToken,
            RefreshToken = refreshToken.Item1,
            RefreshTokenExpiry = refreshToken.Item2,
            UserId = user.Id
        };

        await tokenRepository.AddAsync(token);
        user.JwtTokens.Add(token);

        await userManager.UpdateAsync(user);

        return new Tuple<string?, string?>(
            authToken,
            refreshToken.Item1
        );
    }

    // Refresh the JWT token using the refresh token
    public async Task<Tuple<string?, string?>?> RefreshAsync(string refreshToken)
    {
        var existingToken = await tokenRepository.GetByRefreshTokenAsync(refreshToken);

        if (existingToken == null || existingToken.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token!");

        var user = await userRepository.GetByRefreshTokenAsync(existingToken);

        if (user == null)
            throw new ArgumentException("User not found");

        var generatedAuthToken = await GetAuthToken(user);
        var authToken = new JwtSecurityTokenHandler().WriteToken(generatedAuthToken);
        var generatedRefreshToken = GenerateRefreshToken();

        var token = new JwtToken
        {
            Token = authToken,
            RefreshToken = generatedRefreshToken.Item1,
            RefreshTokenExpiry = generatedRefreshToken.Item2
        };

        await tokenRepository.AddAsync(token);
        user.JwtTokens.Add(token);

        await userManager.UpdateAsync(user);

        return new Tuple<string?, string?>(
            authToken,
            generatedRefreshToken.Item1
        );
    }

    // Get all users
    public async Task<List<UserDto>> GetAllAsync()
    {
        return (await userRepository.GetAllAsync()).Select(ToDto).ToList();
    }

    // Register a new user
    public async Task RegisterAsync(UserRegisterDto user)
    {
        var existingUser = await userManager.FindByNameAsync(user.Username);

        if (existingUser != null)
            throw new Exception("User already exists");

        var newUser = new User
        {
            UserName = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        var result = await userManager.CreateAsync(newUser, user.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create user: {errors}");
        }

        await userManager.AddToRoleAsync(newUser, UserRoles.User);
    }

    // Update an existing user
    public async Task<UserDto?> UpdateAsync(UserDto user)
    {
        var existingUser = await userManager.FindByNameAsync(user.Username);

        if (existingUser == null)
            throw new Exception("User not found");

        var updateResult = await userManager.UpdateAsync(existingUser);

        return updateResult.Succeeded ? ToDto(existingUser) : null;
    }

    // Delete a user by username
    public async Task DeleteAsync(string username)
    {
        var normalizedUserName = userManager.NormalizeName(username);
        var user = await userManager.FindByNameAsync(normalizedUserName);

        if (user == null)
            throw new Exception("User not found");

        await userManager.DeleteAsync(user);
    }

    public async Task AddDeliveryRecipientsAsync(Delivery delivery, List<string> recipientUsernames)
    {
        await userRepository.AddDeliveryRecipientsAsync(delivery, recipientUsernames);
    }

    public static string? CheckTokenValidity(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
            return "Invalid token";

        var jwtToken = tokenHandler.ReadJwtToken(token);
        var expiryClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

        if (expiryClaim == null)
            return "Token does not contain expiry claim";

        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiryClaim.Value)).UtcDateTime;
        return expiryDate < DateTime.UtcNow ? "Expired token" : null;
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(user.UserName, user.FirstName, user.LastName, user.Email, user.PhoneNumber,
            user.IsDeliveryPerson);
    }

    // Generate a JWT token with claims
    private JwtSecurityToken GenerateAuthToken(List<Claim> claims)
    {
        var secretKey = configuration["JWT:Secret"] ?? throw new Exception("Secret key not found");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(6),
            claims: claims,
            signingCredentials: credentials
        );
        return token;
    }

    // Get the JWT token for a user
    private async Task<JwtSecurityToken> GetAuthToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName ?? throw new Exception("User has no username")),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return GenerateAuthToken(claims);
    }

    // Generate a refresh token with an expiry date
    private static Tuple<string, DateTime> GenerateRefreshToken() =>
        new(
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddDays(7)
        );

    // Logout a user by removing their token
    public async Task LogoutAsync(string username)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
            throw new Exception("User not found");

        var token = await tokenRepository.GetTokenByUserId(user.Id);

        await tokenRepository.RemoveAsync(token);
    }
}