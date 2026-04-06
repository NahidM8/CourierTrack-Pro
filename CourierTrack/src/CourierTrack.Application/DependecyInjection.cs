namespace CourierTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}