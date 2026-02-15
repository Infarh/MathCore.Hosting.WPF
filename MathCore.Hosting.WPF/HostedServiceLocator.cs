namespace MathCore.Hosting.WPF;

/// <summary>Базовый класс для реализации локатора сервисов приложения</summary>
/// <example>
/// <code><![CDATA[
/// var locator = new ServiceLocatorHosted();
/// var service = locator.GetRequiredService<IMyService>();
/// ]]></code>
/// </example>
public class ServiceLocatorHosted : ServiceLocator
{
    static ServiceLocatorHosted() => ApplicationHosting.HostBuilderConfiguratorAdd(ConfigureAppServices);

    private static void ConfigureAppServices(IHostBuilder HostBuilder) => HostBuilder.ConfigureServices(ConfigureServices);

    /// <summary>Контейнер сервисов приложения</summary>
    protected override IServiceProvider Services => ApplicationHosting.Services;

    /// <summary>Получить сервис по типу</summary>
    /// <param name="ServiceType">Тип сервиса</param>
    /// <returns>Экземпляр сервиса или <see langword="null"/></returns>
    public object? this[Type ServiceType] => Services.GetService(ServiceType);

    /// <summary>Получить сервис по имени типа</summary>
    /// <param name="ServiceTypeName">Имя типа сервиса</param>
    /// <returns>Экземпляр сервиса или <see langword="null"/></returns>
    public object? this[string ServiceTypeName] => Type.GetType(ServiceTypeName) is { } type ? this[type] : null;

    /// <summary>Получить сервис по типу</summary>
    /// <param name="ServiceType">Тип сервиса</param>
    /// <returns>Экземпляр сервиса или <see langword="null"/></returns>
    public virtual object? GetService(Type ServiceType) => Services.GetService(ServiceType);

    /// <summary>Получить сервис по типу</summary>
    /// <typeparam name="T">Тип сервиса</typeparam>
    /// <returns>Экземпляр сервиса или <see langword="null"/></returns>
    public virtual T? GetService<T>() => (T?)GetService(typeof(T));

    /// <summary>Получить обязательный сервис по типу</summary>
    /// <param name="ServiceType">Тип сервиса</param>
    /// <returns>Экземпляр сервиса</returns>
    public virtual object GetRequiredService(Type ServiceType) => Services.GetRequiredService(ServiceType);

#pragma warning disable CS8714 // Тип не может быть использован как параметр типа в универсальном типе или методе. Допустимость значения NULL для аргумента типа не соответствует ограничению "notnull".
    /// <summary>Получить обязательный сервис по типу</summary>
    /// <typeparam name="T">Тип сервиса</typeparam>
    /// <returns>Экземпляр сервиса</returns>
    public virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();
#pragma warning restore CS8714 // Тип не может быть использован как параметр типа в универсальном типе или методе. Допустимость значения NULL для аргумента типа не соответствует ограничению "notnull".
}