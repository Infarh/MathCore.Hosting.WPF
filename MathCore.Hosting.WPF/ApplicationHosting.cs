using System;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathCore.Hosting.WPF
{
    public abstract class ApplicationHosting : Application
    {
        private static IHost? __Hosting;

        public static IHost Hosting => __Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IHostBuilder CreateHostBuilder(string[] Args) => Host.CreateDefaultBuilder(Args)
           .ConfigureServices((h, s) => (Current as ApplicationHosting)?.ConfigureServices(h, s));

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
