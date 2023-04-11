namespace MyApp;

using System;

/// <summary>
/// Annotates a <c>ResourceDictionary</c> containing localizable strings.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class LocalizedResourcesAttribute : Attribute
{
    /// <summary>
    /// Gets the culture with which the strings will be used. The default value is "en-us".
    /// </summary>
    public string? Culture { get; init; }

    /// <summary>
    /// Gets the path to the Resource Dictionary .axaml file.
    /// The path is relative to the <c>Feature</c> class.
    /// The best practice is to store the .axaml files in the "Locales" directory, and thus an example of usage would be <c>Locales/en-us.axaml</c>.
    /// </summary>
    public string Path { get; init; }
}