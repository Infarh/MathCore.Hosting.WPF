// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Runtime.CompilerServices;

namespace MathCore.Hosting.WPF;

/// <summary>Модель-представления окна</summary>
[AttributeUsage(AttributeTargets.Class)]
public class WindowViewModelAttribute(Type WindowType) : Attribute
{
    public WindowViewModelAttribute() : this(null!) { }

    /// <summary>Тип окна для модели-представления</summary>
    [ConstructorArgument(nameof(WindowType))]
    public required Type WindowType { get; init; } = WindowType;
}

#if NET7_0_OR_GREATER

/// <summary>Модель-представления окна</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class WindowViewModelAttribute<TWindow>() : Attribute where TWindow : Window
{
    /// <summary>Тип окна для модели-представления</summary>
    public Type WindowType { get; } = typeof(TWindow);
}

#endif
