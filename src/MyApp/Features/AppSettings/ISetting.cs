namespace MyApp.Features.AppSettings;

using System.Globalization;

/// <summary>
/// Describes a setting.
/// </summary>
public interface ISetting
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// The setter will cast the value into the target type, and it can throw an exception.
    /// </remarks>
    public object Value { get; set; }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    public object DefaultValue { get; }

    /// <summary>
    /// Deserializes the string value and applies it as the setting's value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <remarks>
    /// Serialization should use <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public void Import(string value);

    /// <summary>
    /// Serializes the setting's value into string.
    /// </summary>
    /// <returns>The string representation.</returns>
    /// <remarks>
    /// Serialization should use <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public string Export();
}