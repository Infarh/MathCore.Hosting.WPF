using System.IO;
using System.Reflection;
using System.Windows;

using MathCore.DI;
using MathCore.WPF.ViewModels;

namespace MathCore.Hosting.WPF;

/// <summary>Приложение WPF с поддержкой механизмов хоста и контейнера сервисов</summary>
public abstract class ApplicationHosting : Application
{
    /// <summary>Событие возникает в момент первичной конфигурации хоста</summary>
    protected static event Action<IHostBuilder>? ConfigureHost;

    private static readonly List<Action<IHostBuilder>> __HostBuilderConfigurations = new();

    /// <summary>Добавить действие конфигурации хоста</summary>
    /// <param name="Configurator">Действие конфигурации</param>
    protected internal static void HostBuilderConfiguratorAdd(Action<IHostBuilder> Configurator) => __HostBuilderConfigurations.Add(Configurator);

    /// <summary>Удалить действие конфигурации хоста</summary>
    /// <param name="Configurator">Действие конфигурации</param>
    /// <returns>Истина, если действие конфигурации хоста удалено успешно</returns>
    protected static bool HostBuilderConfiguratorRemove(Action<IHostBuilder> Configurator) => __HostBuilderConfigurations.Remove(Configurator);

    /// <summary>Удалить все действия конфигурации хоста</summary>
    protected static void HostBuilderConfiguratorClear() => __HostBuilderConfigurations.Clear();

    /// <summary>Событие, возникающее при инициализации хоста для добавления сервисов в контейнер</summary>
    protected static event Action<HostBuilderContext, IServiceCollection>? ConfigureServices;

    private static readonly List<Action<HostBuilderContext, IServiceCollection>> __ServicesConfigurators = new()
    {
        LoadingServiceFromExecutingAssembly,
    };

    public static IReadOnlyList<Assembly> ErrorLoadingServicesAssemblies { get; private set; } = Array.Empty<Assembly>();

    private static void LoadingServiceFromExecutingAssembly(HostBuilderContext Host, IServiceCollection services)
    {
        var error_assemblies = new List<Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            try
            {
                if (assembly.GetName() is { Name: { Length: > 0 } name } && (name.StartsWith("Microsoft") || name.Contains("Interop") || name.Contains("Blend")))
                    continue;

                if (assembly.GetCustomAttributes<AssemblyCompanyAttribute>().FirstOrDefault() is { Company: { Length: > 0 } company } && company.Contains("Microsoft"))
                    continue;

                if (assembly.DefinedTypes.Any(type => type.GetMethod("Main") != null))
                {
                    services.AddServicesFromAssembly(assembly);
                    continue;
                }

                if (assembly.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Service")))
                    services.AddServicesFromAssembly(assembly);
            }
            catch (ReflectionTypeLoadException)
            {
                error_assemblies.Add(assembly);
            }

        if (error_assemblies.Count == 0) return;
        error_assemblies.TrimExcess();
        ErrorLoadingServicesAssemblies = error_assemblies.ToArray();
    }

    /// <summary>Добавить действие конфигурации коллекции сервисов</summary>
    /// <param name="Configurator">Действие конфигурации сервисов</param>
    protected static void ServicesAdd(Action<HostBuilderContext, IServiceCollection> Configurator) => __ServicesConfigurators.Add(Configurator);

    /// <summary>Удалить действие конфигурации сервисов</summary>
    /// <param name="Configurator">Действие конфигурации сервисов</param>
    /// <returns>Истина, если действие конфигурации сервисов удалено успешно</returns>
    protected static bool ServicesRemove(Action<HostBuilderContext, IServiceCollection> Configurator) => __ServicesConfigurators.Remove(Configurator);

    /// <summary>Очистить все действия конфигурации сервисов</summary>
    protected static void ServicesClear() => __ServicesConfigurators.Clear();

    /// <summary>Текущее окно в фокусе</summary>
    public static Window? FocusedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);

    /// <summary>Текущее активное окно</summary>
    public static Window? ActiveWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);

    /// <summary>Текущее окно</summary>
    public static Window? CurrentWindow => FocusedWindow ?? ActiveWindow ?? Current.MainWindow;

    private static IHost? __Hosting;

    /// <summary>Хост приложения</summary>
    public static IHost Hosting => __Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs())
       .AddServiceLocator()
       .Build();

    /// <summary>Контейнер сервисов приложения</summary>
    public static IServiceProvider Services => Hosting.Services;

    /// <summary>Конфигурация приложения</summary>
    public static IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    /// <summary>Конфигурация построителя хоста</summary>
    /// <param name="Args">Аргументы командной строки</param>
    /// <returns>Сконфигурированный построитель хоста</returns>
    public static IHostBuilder CreateHostBuilder(string[] Args)
    {
        var builder = Host.CreateDefaultBuilder(Args);
        foreach (var configurator in __HostBuilderConfigurations)
            configurator(builder);

        ConfigureHost?.Invoke(builder);

        foreach (var configurator in __ServicesConfigurators)
            builder.ConfigureServices(configurator);

        if (ConfigureServices is { } register_services_handlers)
            builder.ConfigureServices(register_services_handlers);

        return builder;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var host = Hosting;
        Resources["ServiceLocator"] = new ServiceLocatorHosted();
        base.OnStartup(e);
        // ReSharper disable once AsyncApostle.AsyncAwaitMayBeElidedHighlighting
        await host.StartAsync().ConfigureAwait(false);

        __HostBuilderConfigurations.Clear();
        __ServicesConfigurators.Clear();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using var host = Hosting;
        base.OnExit(e);
        await host.StopAsync().ConfigureAwait(false);
    }
}