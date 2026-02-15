#if !NET7_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>Атрибут обязательного члена</summary>
/// <example>
/// <code><![CDATA[
/// public sealed class Person
/// {
///     [RequiredMember]
///     public string Name { get; init; }
/// }
/// ]]></code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
internal sealed class RequiredMemberAttribute : Attribute { }

/// <summary>Атрибут требования возможностей компилятора</summary>
/// <example>
/// <code><![CDATA[
/// [CompilerFeatureRequired(CompilerFeatureRequiredAttribute.RequiredMembers)]
/// public sealed class FeatureDependentType { }
/// ]]></code>
/// </example>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    /// <summary>Имя возможности обязательных членов</summary>
    public const string RequiredMembers = nameof(RequiredMembers);

    /// <summary>Имя возможности ссылочных структур</summary>
    public const string RefStructs = nameof(RefStructs);

    /// <summary>Имя требуемой возможности</summary>
    public string FeatureName { get; }

    /// <summary>Признак необязательной возможности</summary>
    public bool IsOptional { get; init; }

    /// <summary>Создать атрибут требования возможностей компилятора</summary>
    /// <param name="FeatureName">Имя требуемой возможности</param>
    public CompilerFeatureRequiredAttribute(string FeatureName) => this.FeatureName = FeatureName;
}

/// <summary>Маркер инициализации внешнего члена</summary>
/// <example>
/// <code><![CDATA[
/// public sealed class Person
/// {
///     public string Name { get; init; }
/// }
/// ]]></code>
/// </example>
internal sealed class IsExternalInit { }

#endif