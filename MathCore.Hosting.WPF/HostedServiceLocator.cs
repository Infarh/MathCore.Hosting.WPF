using System;

namespace MathCore.Hosting.WPF
{
    public class ServiceLocatorHosted : ServiceLocator
    {
        protected override IServiceProvider Services => ApplicationHosting.Services;
    }
}
