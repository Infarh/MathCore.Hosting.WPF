using Microsoft.Extensions.DependencyInjection;

namespace MathCore.Hosting.WPF;

public class ServiceLocatorHosted : ServiceLocator
{
    protected override IServiceProvider Services => ApplicationHosting.Services;

    public virtual object? GetService(Type ServiceType) => Services.GetService(ServiceType);

    public virtual T? GetService<T>() => (T?)GetService(typeof(T));

    public virtual object GetRequiredService(Type ServiceType) => Services.GetRequiredService(ServiceType);

#pragma warning disable CS8714 // Тип не может быть использован как параметр типа в универсальном типе или методе. Допустимость значения NULL для аргумента типа не соответствует ограничению "notnull".
    public virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();
#pragma warning restore CS8714 // Тип не может быть использован как параметр типа в универсальном типе или методе. Допустимость значения NULL для аргумента типа не соответствует ограничению "notnull".
}