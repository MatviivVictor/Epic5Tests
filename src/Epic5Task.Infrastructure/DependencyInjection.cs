using Epic5Task.Application.Interfaces;
using Epic5Task.Infrastructure.DataProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Epic5Task.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEventProvider, EventProvider>();
        services.AddScoped<IUserProvider, UserProvider>();
        services.AddScoped<ITicketProvider, TicketProvider>();
        
        return services;
    }
}