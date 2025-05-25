using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Service;

namespace Application.Util;

public static class ServiceConfig
{
    private static readonly ILogger Logger;

    static ServiceConfig()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });
        
        Logger = loggerFactory.CreateLogger("ServiceConfigLogger");
    }
    
    public static IServiceCollection AddRepositoryConfig(this IServiceCollection services)
    {
        services.AddScoped<UserRepository>();
        services.AddScoped<TokenRepository>();
        services.AddScoped<LocationRepository>();
        services.AddScoped<DeliveryRepository>();
        return services;
    }

    public static IServiceCollection AddServiceConfig(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<LocationService>();
        services.AddScoped<DeliveryService>();
        return services;
    }

    public static IServiceCollection AddAuthenticationConfig
    (this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtIssuer = configuration["JWT:Issuer"] ?? string.Empty;
        var jwtAudience = configuration["JWT:Audience"] ?? string.Empty;
        var jwtSecret = configuration["JWT:Secret"] ?? string.Empty;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = GetTokenValidationParameters(jwtIssuer, jwtAudience, jwtSecret);
                options.Events = GetJwtBearerEvents();
                options.IncludeErrorDetails = true;
            });

        return services;
    }

    private static TokenValidationParameters GetTokenValidationParameters(string jwtIssuer, string jwtAudience,
        string jwtSecret)
    {
        var encodedJwtSecret = Encoding.UTF8.GetBytes(jwtSecret);
        return new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(encodedJwtSecret)
        };
    }

    private static JwtBearerEvents GetJwtBearerEvents() =>
        new()
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (string.IsNullOrEmpty(accessToken) || path.Value == null || !path.Value.Contains("/hubs/location"))
                    return Task.CompletedTask;
                context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
}