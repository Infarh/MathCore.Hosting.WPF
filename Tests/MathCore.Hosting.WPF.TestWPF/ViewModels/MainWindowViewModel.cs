using MathCore.DI;
using MathCore.WPF.ViewModels;

namespace MathCore.Hosting.WPF.TestWPF.ViewModels;

[Service]
public class MainWindowViewModel : TitledViewModel
{
    public MainWindowViewModel() => Title = "Test main window";
}