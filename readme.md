# MathCore.Hosting.WPF

`MathCore.Hosting.WPF` – вспомогательный пакет для интеграции механизма `Generic Host` и контейнера DI (`Microsoft.Extensions.DependencyInjection`) в WPF‑приложение.
Пакет упрощает:
- построение и запуск `IHost` внутри WPF `Application`
- регистрацию и получение сервисов через статический локатор
- автозагрузку сервисов из сборок
- доступ к `IConfiguration` и окружению хоста

## Возможности
- Базовый класс `ApplicationHosting` для наследования вместо стандартного `Application`
- Статические события/методы для настройки хоста и регистрации сервисов
- Автоматическое сканирование сборок и добавление сервисов (по эвристике)
- Ресурс `ServiceLocator` доступный из XAML и кода
- Поддержка многих целевых платформ (.NET 6–10 + .NET Framework 4.6.1–4.8)

## Установка
```bash
# Через .NET CLI
dotnet add package MathCore.Hosting.WPF

# Через Package Manager
Install-Package MathCore.Hosting.WPF
```

## Быстрый старт
### 1. Наследуемся от `ApplicationHosting`
`App.xaml.cs`:
```csharp
using MathCore.Hosting.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyApp;

public interface IMyService { void Do(); }
public class MyService : IMyService { public void Do() { /* логика */ } }

public partial class App : ApplicationHosting
{
    static App()
    {
        // Подписка единожды – при загрузке типа
        ConfigureServices += OnConfigureServices;
    }

    private static void OnConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Регистрация пользовательских сервисов
        services.AddSingleton<IMyService, MyService>();
    }
}
```

### 2. Меняем корневой тег `App.xaml`
```xml
<local:App x:Class="MyApp.App"
           xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:local="clr-namespace:MyApp"
           StartupUri="MainWindow.xaml">
    <!-- Ресурсы приложения -->
</local:App>
```

### 3. Используем сервисы в окне
```csharp
using Microsoft.Extensions.DependencyInjection;
using MathCore.Hosting.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var my_service = ApplicationHosting.Services.GetRequiredService<IMyService>();
        my_service.Do();
    }
}
```

## Альтернативные способы регистрации
Помимо события `ConfigureServices` можно:
```csharp
// Добавить конфигуратор программно (например, в статическом конструкторе App)
ServicesAdd((context, services) =>
{
    services.AddSingleton<IOtherService, OtherService>();
});

// Удалить ранее добавленный конфигуратор
ServicesRemove(myConfigurator);

// Очистить все пользовательские конфигураторы
ServicesClear();
```

Для настройки самого `HostBuilder`:
```csharp
HostBuilderConfiguratorAdd(builder =>
{
    builder.ConfigureLogging(logging =>
    {
        logging.AddDebug();
    });
});
```

## Получение сервисов в XAML через ресурс локатора
После запуска приложение добавляет ресурс:
```xaml
{StaticResource ServiceLocator}
```
Пример привязки (через `ObjectDataProvider` или конвертер) – обычно предпочтительнее получать сервисы в коде/VM.

## Автозагрузка сервисов из сборок
Встроенный конфигуратор автоматически вызывает `services.AddServicesFromAssembly(assembly)` для сборок:
- содержащих тип с методом `Main`
- имеющих атрибуты, имя которых включает слово `Service`

Исключаются системные/Interop/Blend и сборки компании Microsoft. Сборки, не прошедшие загрузку типов (`ReflectionTypeLoadException`), сохраняются в `ApplicationHosting.ErrorLoadingServicesAssemblies`.

## Доступ к окружению и конфигурации
```csharp
var configuration = ApplicationHosting.Configuration; // IConfiguration
var host = ApplicationHosting.Hosting;                // IHost
var provider = ApplicationHosting.Services;           // IServiceProvider
```

## Жизненный цикл
- `OnStartup` автоматически строит и запускает `IHost`
- `OnExit` останавливает и освобождает хост
- После запуска внутренние списки конфигураторов очищаются (чтобы избежать повторной регистрации при переразборе XAML / design‑time)

## Рекомендации
- Подписывайтесь на `ConfigureServices` только один раз в статическом конструкторе приложения
- Избегайте долгих операций в конфигурировании – переносите их в фоновые службы (`IHostedService`)
- Для тестов можно создавать временный `HostBuilder` через `CreateHostBuilder(args)`

## Пример фоновой службы
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    public Worker(ILogger<Worker> logger) => _logger = logger; // внедрение через DI

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Tick {Time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}

// Регистрация
ConfigureServices += (_, services) => services.AddHostedService<Worker>();
```

## Лицензия
Проект распространяется под лицензией автора (см. файл LICENSE при наличии)

## Обратная связь
Issue / идеи / предложения: раздел Issues репозитория GitHub.
