#nullable enable
using System;

using MathCore.Hosting.WPF.TestWPF.ViewModels;

namespace MathCore.Hosting.WPF.TestWPF;

public class ServiceLocator : ServiceLocatorHosted
{
    public Exception? LastError { get; private set; }

    public MainWindowViewModel MainModel
    {
        get
        {
            try
            {
                return GetRequiredService<MainWindowViewModel>();
            }
            catch (Exception error)
            {
                LastError = error;
                throw;
            }
        }
    }
}