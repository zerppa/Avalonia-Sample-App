namespace MyApp;

using System;

/// <summary>
/// Declares that the property is resolved as dependency.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public sealed class DependencyAttribute : Attribute
{
}