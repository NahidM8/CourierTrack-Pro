using CourierTrack.Application.Options;
using Microsoft.Extensions.Configuration;

namespace CourierTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
        services.AddScoped<IOrderService, OrderService>();
        services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));
        return services;
    }
}