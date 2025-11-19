using System.Diagnostics.CodeAnalysis;
using MathCore.DI;
// ReSharper disable EventNeverSubscribedTo.Global

namespace MathCore.Hosting.WPF;

/// <summary>Приложение WPF с поддержкой механизмов хоста и контейнера сервисов</summary>
/// <remarks>
/// Для использования необходимо унаследовать ваш класс приложения от данного абстрактного класса и корректно указать его в корневой разметке App.xaml
/// Регистрацию пользовательских сервисов можно выполнить через статическое событие ConfigureServices или методы ServicesAdd/ServicesRemove
/// </remarks>
/// <example>
/// Пример настройки приложения:
/// 1. Создаём класс App.xaml.cs, наследуя его от ApplicationHosting:
/// <code>
/// using MathCore.Hosting.WPF;
/// using Microsoft.Extensions.DependencyInjection;
/// using Microsoft.Extensions.Hosting;
///
/// namespace MyApp;
///
/// public interface IMyService { void Do(); }
/// public class MyService : IMyService { public void Do() { /* реализация */ } }
///
/// public partial class App : ApplicationHosting
/// {
///     static App()
///     {
///         // Подписка на событие конфигурации сервисов один раз при загрузке типа
///         ConfigureServices += OnConfigureServices;
///     }
///
///     private static void OnConfigureServices(HostBuilderContext context, IServiceCollection services)
///     {
///         // Регистрация собственных сервисов приложения
///         services.AddSingleton<IMyService, MyService>();
///     }
/// }
/// </code>
/// 2. Изменяем корень файла App.xaml, указывая локальный класс (унаследован от ApplicationHosting):
/// <code>
/// <local:App x:Class="MyApp.App"
///            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
///            xmlns:local="clr-namespace:MyApp"
///            StartupUri="MainWindow.xaml">
///     <!-- Ресурсы приложения -->
/// </local:App>
/// </code>
/// 3. Использование зарегистрированного сервиса в окне:
/// <code>
/// using Microsoft.Extensions.DependencyInjection;
///
/// public partial class MainWindow : Window
/// {
///     public MainWindow()
///     {
///         InitializeComponent();
///         var my_service = ApplicationHosting.Services.GetRequiredService<IMyService>();
///         my_service.Do();
///     }
/// }
/// </code>
/// </example>
public abstract class ApplicationHosting : Application
{
    /// <summary>Событие возникает в момент первичной конфигурации хоста</summary>
    protected static event Action<IHostBuilder>? ConfigureHost;

    private static readonly List<Action<IHostBuilder>> __HostBuilderConfigurations = [];

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

    private static readonly List<Action<HostBuilderContext, IServiceCollection>> __ServicesConfigurators =
    [
        LoadingServiceFromExecutingAssembly,
    ];

    /// <summary>Список сборок, в которых произошли ошибки при загрузке сервисов</summary>
    public static IReadOnlyList<Assembly> ErrorLoadingServicesAssemblies { get; private set; } = [];

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

    /// <summary>Хост приложения</summary>
    [field: MaybeNull, AllowNull]
    public static IHost Hosting => field ??= CreateHostBuilder(Environment.GetCommandLineArgs())
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

        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton(context.HostingEnvironment);
            services.AddSingleton(_ => Current);
        });

        ConfigureHost?.Invoke(builder);

        foreach (var configurator in __ServicesConfigurators)
            builder.ConfigureServices(configurator);

        if (ConfigureServices is { } register_services_handlers)
            builder.ConfigureServices(register_services_handlers);

        return builder;
    }

    /// <summary>Переопределяет логику старта приложения для инициализации хоста и контейнера сервисов</summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            var host = Hosting;
            Resources["ServiceLocator"] = new ServiceLocatorHosted();
            base.OnStartup(e);
            // ReSharper disable once AsyncApostle.AsyncAwaitMayBeElidedHighlighting
            await host.StartAsync().ConfigureAwait(false);

            __HostBuilderConfigurations.Clear();
            __ServicesConfigurators.Clear();
        }
        catch (Exception error)
        {
            if(!HandleStartupException(error))
                // ReSharper disable once AsyncVoidThrowException
                throw;
        }
    }

    /// <summary>
    /// Переопределяет логику обработки исключений, возникающих при старте приложения
    /// </summary>
    /// <param name="error">Возникшее в процессе выполнения метода <see cref="OnStartup"/> исключение</param>
    /// <returns>
    /// Истина, если исключение обработано и его повторная генерация не требуется - приложение продолжит работать;
    /// Ложь, если исключение не обработано и его необходимо повторно сгенерировать - приложение завершит работу.
    /// </returns>
    protected virtual bool HandleStartupException(Exception error) => false;

    /// <summary>Переопределяет логику завершения приложения для корректной остановки хоста</summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            using var host = Hosting;
            base.OnExit(e);
            await host.StopAsync().ConfigureAwait(false);
        }
        catch (Exception error)
        {
            if(!HandleExitException(error))
                // ReSharper disable once AsyncVoidThrowException
                throw;
        }
    }

    /// <summary>
    /// Переопределяет логику обработки исключений, возникающих при завершении приложения
    /// </summary>
    /// <param name="error">Возникшее в процессе выполнения метода <see cref="OnExit"/> исключение</param>
    /// <returns>
    /// Истина, если исключение обработано и его повторная генерация не требуется - приложение продолжит завершение работы;
    /// Ложь, если исключение не обработано и его необходимо повторно сгенерировать - приложение завершит работу с ошибкой.
    /// </returns>
    protected virtual bool HandleExitException(Exception error) => false;
}