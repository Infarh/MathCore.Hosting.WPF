// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
namespace MathCore.Hosting.WPF;

/// <summary>Модель-представления окна</summary>
public class WindowViewModelAttribute : Attribute
{
    /// <summary>Тип окна для модели-представления</summary>
    [ConstructorArgument(nameof(WindowType))]
    public Type WindowType { get; set; }

    public WindowViewModelAttribute() { }

    public WindowViewModelAttribute(Type WindowType) => this.WindowType = WindowType;
}

#if NET7

/// <summary>Модель-представления окна</summary>
public sealed class WindowViewModelAttribute<TWindow> : WindowViewModelAttribute where TWindow : Window
{
    public WindowViewModelAttribute() : base(typeof(TWindow)) { }
}

#endif
