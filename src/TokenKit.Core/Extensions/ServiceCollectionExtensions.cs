using Microsoft.Extensions.DependencyInjection;
using TokenKit.Core.Implementations;
using TokenKit.Core.Interfaces;

namespace TokenKit.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers TokenKit.Core services. If <paramref name="jsonPath"/> is null,
    /// the registry will look for models.data.json in AppContext.BaseDirectory.
    /// </summary>
    public static IServiceCollection AddTokenKitCore(this IServiceCollection services, string? jsonPath = null)
    {
        services.AddSingleton<IModelRegistry>(_ => new JsonModelRegistry(jsonPath));
        services.AddSingleton<ICostEstimator, BasicCostEstimator>();
        services.AddSingleton<ITokenKitCore, TokenKitCore>();
        return services;
    }
}

