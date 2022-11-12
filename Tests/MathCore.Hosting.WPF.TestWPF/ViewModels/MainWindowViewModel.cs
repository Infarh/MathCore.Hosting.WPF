namespace MathCore.Hosting.WPF.TestWPF.ViewModels;

[Service, WindowViewModel<MainWindow>]
public class MainWindowViewModel : TitledViewModel
{
    public MainWindowViewModel() => Title = "Test main window";
}