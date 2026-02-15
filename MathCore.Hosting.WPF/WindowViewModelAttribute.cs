// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Runtime.CompilerServices;

namespace MathCore.Hosting.WPF;

/// <summary>Модель-представления окна</summary>
/// <param name="WindowType">Тип окна для модели-представления</param>
/// <example>
/// <code><![CDATA[
/// [WindowViewModel(typeof(MainWindow))]
/// public sealed class MainWindowViewModel : ViewModel { }
/// ]]></code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public class WindowViewModelAttribute(Type WindowType) : Attribute
{
    /// <summary>Создать атрибут без указания типа окна</summary>
    public WindowViewModelAttribute() : this(null!) { }

    /// <summary>Тип окна для модели-представления</summary>
    [ConstructorArgument(nameof(WindowType))]
    public required Type WindowType { get; init; } = WindowType;
}

#if NET7_0_OR_GREATER

/// <summary>Модель-представления окна</summary>
/// <typeparam name="TWindow">Тип окна</typeparam>
/// <example>
/// <code><![CDATA[
/// [WindowViewModel<MainWindow>]
/// public sealed class MainWindowViewModel : ViewModel { }
/// ]]></code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class WindowViewModelAttribute<TWindow>() : Attribute where TWindow : Window
{
    /// <summary>Тип окна для модели-представления</summary>
    public Type WindowType { get; } = typeof(TWindow);
}

#endif
