using Epic5Task.Application.Commons.Providers;
using Epic5Task.Application.Interfaces;
using Epic5Task.Application.PipelineBehaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Epic5Task.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<IUserContextProvider, UserContextProvider>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        
        // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
    
    
}