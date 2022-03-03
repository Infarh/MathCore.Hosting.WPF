using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathCore.Hosting.WPF;

/// <summary>Базовый класс для реализации локатора сервисов приложения</summary>
public class ServiceLocatorHosted : ServiceLocator
{
    static ServiceLocatorHosted() => ApplicationHosting.HostBuilderConfiguratorAdd(ConfigureAppServices);

    private static void ConfigureAppServices(IHostBuilder HostBuilder) => HostBuilder.ConfigureServices(ConfigureServices);

    protected override IServiceProvider Services => ApplicationHosting.Services;

    public object? this[Type ServiceType] => Services.GetService(ServiceType);

    public object? this[string ServiceTypeName] => Type.GetType(ServiceTypeName) is { } type ? this[type] : null;

    public virtual object? GetService(Type ServiceType) => Services.GetService(ServiceType);

    public virtual T? GetService<T>() => (T?)GetService(typeof(T));

    public virtual object GetRequiredService(Type ServiceType) => Services.GetRequiredService(ServiceType);

#pragma warning disable CS8714 // Тип не может быть использован как параметр типа в универсальном типе или методе. Допустимость значения NULL для аргумента типа не соответствует ограничению "notnull".
    public virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();
#pragma warning restore CS8714 // Тип не может быть использован как параметр типа в универсальном типе или методе. Допустимость значения NULL для аргумента типа не соответствует ограничению "notnull".
}