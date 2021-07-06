using System;
using System.Linq;
using System.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathCore.Hosting.WPF
{
    public abstract class ApplicationHosting : Application
    {
        public static Window? FocusedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);
        public static Window? ActiveWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
        public static Window? CurrentWindow => FocusedWindow ?? ActiveWindow ?? Current.MainWindow;

        private static IHost? __Hosting;

        public static IHost Hosting => __Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IServiceProvider Services => Hosting.Services;

        public static IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

        public static IHostBuilder CreateHostBuilder(string[] Args)
        {
            var builder = Host.CreateDefaultBuilder(Args);
            var app = Current as ApplicationHosting;
            builder = app?.ConfigureHostBuilder(builder) ?? builder;
            builder.ConfigureServices((h, s) => (Current as ApplicationHosting)?.ConfigureServices(h, s));
            return app?.ConfigureHostBuilderFinal(builder) ?? builder;
        }

        protected virtual IHostBuilder ConfigureHostBuilder(IHostBuilder builder) => builder;

        protected virtual IHostBuilder ConfigureHostBuilderFinal(IHostBuilder builder) => builder;

        protected virtual void ConfigureServices(HostBuilderContext host, IServiceCollection services) => 
            services.AddServicesFromAssembly(GetType());

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
}
