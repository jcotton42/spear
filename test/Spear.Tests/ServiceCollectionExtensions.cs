using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Spear.Tests;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddScopedMocked<T>(this IServiceCollection services) where T : class {
        return services
            .AddScoped<Mock<T>>(_ => new Mock<T>(MockBehavior.Strict))
            .AddScoped<T>(sp => sp.GetRequiredService<Mock<T>>().Object);
    }
}
