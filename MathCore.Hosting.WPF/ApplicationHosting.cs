using System.Reflection;
using System.Windows;

using MathCore.DI;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathCore.Hosting.WPF;

public abstract class ApplicationHosting : Application
{
    protected static event Action<IHostBuilder>? ConfigureHost;

    private static readonly List<Action<IHostBuilder>> __HostBuilderConfigurations = new();

    protected static void HostBuilderConfiguratorAdd(Action<IHostBuilder> Configurator) => __HostBuilderConfigurations.Add(Configurator);
    protected static bool HostBuilderConfiguratorRemove(Action<IHostBuilder> Configurator) => __HostBuilderConfigurations.Remove(Configurator);
    protected static void HostBuilderConfiguratorClear() => __HostBuilderConfigurations.Clear();

    protected static event Action<HostBuilderContext, IServiceCollection>? ConfigureServices;

    private static readonly List<Action<HostBuilderContext, IServiceCollection>> __ServicesConfigurators = new()
    {
        LoadingServiceFromExecutingAssembly,
    };

    private static void LoadingServiceFromExecutingAssembly(HostBuilderContext Host, IServiceCollection services)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name is { Length: > 0 } name && (name.Contains("Microsoft") || name.Contains("Interop") || name.Contains("Blend")))
                continue;
            if (assembly.GetCustomAttributes<AssemblyCompanyAttribute>().FirstOrDefault() is { } attribute && attribute.Company.Contains("Microsoft"))
                continue;

            if (assembly.DefinedTypes.Any(type => type.GetMethod("Main") != null))
            {
                services.AddServicesFromAssembly(assembly);
                continue;
            }

            if(assembly.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Service")))
                services.AddServicesFromAssembly(assembly);
        }
    }

    protected static void SrervicesAdd(Action<HostBuilderContext, IServiceCollection> Configurator) => __ServicesConfigurators.Add(Configurator);
    protected static bool SrervicesRemove(Action<HostBuilderContext, IServiceCollection> Configurator) => __ServicesConfigurators.Remove(Configurator);
    protected static void SrervicesClear() => __ServicesConfigurators.Clear();

    public static Window? FocusedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);
    public static Window? ActiveWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
    public static Window? CurrentWindow => FocusedWindow ?? ActiveWindow ?? Current.MainWindow;

    private static IHost? __Hosting;

    public static IHost Hosting => __Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs())
       .AddServiceLocator()
       .Build();

    public static IServiceProvider Services => Hosting.Services;

    public static IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

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
        base.OnStartup(e);
        await host.StartAsync().ConfigureAwait(false);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using var host = Hosting;
        base.OnExit(e);
        await host.StopAsync().ConfigureAwait(false);
    }
}