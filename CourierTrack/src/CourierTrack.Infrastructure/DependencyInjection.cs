using CourierTrack.Infrastructure.Repositories.Implementations;
using CourierTrack.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourierTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<CourierTrackDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.Lockout.AllowedForNewUsers = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<CourierTrackDbContext>()
        .AddDefaultTokenProviders()
        .AddUserManager<UserManager<User>>()
        .AddSignInManager();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourierRepository, CourierRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICourierRatingRepository, CourierRatingRepository>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
