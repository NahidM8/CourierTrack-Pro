using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace CourierTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);
        return services;
    }
}