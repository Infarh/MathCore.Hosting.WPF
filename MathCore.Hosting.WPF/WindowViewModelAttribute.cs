// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace MathCore.Hosting.WPF;

/// <summary>Модель-представления окна</summary>
public class WindowViewModelAttribute : Attribute
{
    /// <summary>Тип окна для модели-представления</summary>
    [ConstructorArgument(nameof(WindowType))]
    public required Type WindowType { get; init; }

    public WindowViewModelAttribute() { }

    public WindowViewModelAttribute(Type WindowType) => this.WindowType = WindowType;
}

#if NET7

/// <summary>Модель-представления окна</summary>
public sealed class WindowViewModelAttribute<TWindow> : Attribute where TWindow : Window
{
    /// <summary>Тип окна для модели-представления</summary>
    public Type WindowType { get; }

    public WindowViewModelAttribute() => WindowType = typeof(TWindow);
}

#endif
