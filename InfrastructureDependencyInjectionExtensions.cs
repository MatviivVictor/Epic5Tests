using Microsoft.Extensions.DependencyInjection;

namespace Epic5Task.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Тут пізніше: EF Core DbContext, репозиторії, зовнішні інтеграції тощо
        return services;
    }
}
