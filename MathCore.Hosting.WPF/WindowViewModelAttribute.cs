using System.Windows.Markup;

namespace MathCore.Hosting.WPF;

public sealed class WindowViewModelAttribute : Attribute
{
    [ConstructorArgument(nameof(WindowType))]
    public Type WindowType { get; set; }

    public WindowViewModelAttribute() { }

    public WindowViewModelAttribute(Type WindowType) => this.WindowType = WindowType;
}