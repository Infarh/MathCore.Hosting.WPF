# MathCore.Hosting.WPF

Инфраструктурный NuGet‑пакет для интеграции `Generic Host` (`Microsoft.Extensions.Hosting`) и DI (`Microsoft.Extensions.DependencyInjection`) в WPF‑приложения без ручного шаблонного кода.

## Задачи которые решает
- Единый `IHost` внутри WPF `Application`
- Простая регистрация сервисов и фоновых служб
- Автоматическое сканирование сборок и подхват сервисов
- Удобный доступ к `IHost`, `IServiceProvider`, `IConfiguration`
- Встраиваемый `ServiceLocator` как ресурс XAML

## Установка
```bash
dotnet add package MathCore.Hosting.WPF
```

## Быстрый старт
1. Наследуйтесь от `ApplicationHosting`:
```csharp
using MathCore.Hosting.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyApp;

public interface IMyService { void Do(); }
public class MyService : IMyService { public void Do() { /* логика */ } }

public partial class App : ApplicationHosting
{
    static App() => ConfigureServices += OnConfigureServices; // подписка один раз при загрузке типа

    private static void OnConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>(); // регистрация сервисов
    }
}
```
2. Корневой тег `App.xaml`:
```xml
<local:App x:Class="MyApp.App"
           xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:local="clr-namespace:MyApp"
           StartupUri="MainWindow.xaml" />
```
3. Использование сервиса в окне:
```csharp
using Microsoft.Extensions.DependencyInjection;
using MathCore.Hosting.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var svc = ApplicationHosting.Services.GetRequiredService<IMyService>();
        svc.Do();
    }
}
```

## Регистрация через методы
```csharp
// Добавление (например в статическом конструкторе App)
ServicesAdd((ctx, services) => services.AddSingleton<IOtherService, OtherService>());

// Удаление
ServicesRemove(myConfigurator);

// Очистка
ServicesClear();
```

## Настройка HostBuilder
```csharp
HostBuilderConfiguratorAdd(builder =>
{
    builder.ConfigureLogging(logging => logging.AddDebug());
});
```

## Фоновая служба пример
```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _Logger;
    public Worker(ILogger<Worker> logger) => _Logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            _Logger.LogInformation("Tick {Time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}

ConfigureServices += (_, services) => services.AddHostedService<Worker>();
```

## Доступ к инфраструктуре
```csharp
var host          = ApplicationHosting.Hosting;        // IHost
var services      = ApplicationHosting.Services;       // IServiceProvider
var configuration = ApplicationHosting.Configuration;  // IConfiguration
```

## Авто‑регистрация сервисов
Пакет пытается вызвать `services.AddServicesFromAssembly(assembly)` для сборок:
- содержащих тип с методом `Main`
- имеющих атрибуты с именем содержащим `Service`
(Системные/Interop/Blend/Microsoft сборки пропускаются.) Ошибочные сборки сохраняются в `ErrorLoadingServicesAssemblies`.

## Жизненный цикл
- `OnStartup` строит и запускает хост
- `OnExit` корректно останавливает хост
- После запуска списки конфигураторов очищаются чтобы избежать повторной регистрации

## Советы
- Подписывайтесь на события один раз (статический конструктор)
- Долгие операции выносите в `BackgroundService`
- Используйте DI для ViewModel и служб вместо прямого ServiceLocator

## Цели
.NET 6–10, .NET Framework 4.6.1–4.8

## Лицензия
MIT
