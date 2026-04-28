namespace CourierTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICourierService, CourierService>();
        services.AddScoped<ICourierRatingService, CourierRatingService>();
        services.AddScoped<IOrderAssignmentService, OrderAssignmentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IUserService, UserService>();
        services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));
        return services;
    }
}