using Microsoft.Extensions.DependencyInjection;

namespace MathCore.Hosting.WPF.TestWPF
{
    public partial class App
    {
        static App() => ConfigureServices += (_, services) => services.AddSingleton(_ => Current);
    }
}
