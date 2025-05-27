using Application.Hubs;
using Application.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model;
using Npgsql;
using Repository.Context;

namespace Application;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder
            .Configuration
            .GetConnectionString("DefaultConnection")
            ?.Replace("${DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST"))
            .Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT"))
            .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"))
            .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER"))
            .Replace("${DB_PASS}", Environment.GetEnvironmentVariable("DB_PASS"));

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<DeliveryCategory>();
        var dataSource = dataSourceBuilder.Build();

        // Add database context
        builder.Services.AddDbContext<DelivereaseDbContext>(options => options.UseNpgsql(dataSource));

        // Add identity parameters
        builder.Services.AddIdentityCore<User>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<DelivereaseDbContext>()
            .AddDefaultTokenProviders();

        // Add CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policyBuilder =>
            {
                policyBuilder
                    .WithOrigins(
                        "http://localhost",
                        "http://localhost:3000",
                        "https://deliverease-web-git-deploy-stoyans-projects-cafee331.vercel.app",
                        "https://schorbov.eu"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Add repositories and services
        builder.Services
            .AddRepositoryConfig()
            .AddServiceConfig();

        // Add auth
        builder.Services.AddAuthenticationConfig(builder.Configuration);
        builder.Services.AddAuthorization();

        // Add real-time communication
        builder.Services.AddSignalR();

        // Register controller
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Limit to http only
        builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5000); });

        var app = builder.Build();

        app.UseCors("CorsPolicy");

        // Add roles to database
        await SeedDatabase(app.Services);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapHub<LocationsHub>("/hubs/locations");

        app.MapControllers();

        await app.RunAsync();
    }

    // Seed database with default roles
    private static async Task SeedDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DelivereaseDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await context.Database.MigrateAsync();

        if (await context.Roles.AnyAsync())
            return;

        await roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.User));
        await roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.Admin));
        await context.SaveChangesAsync();
    }
}