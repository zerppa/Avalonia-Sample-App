namespace MyApp.Features.AppSettings;

using System;
using System.Globalization;

/// <summary>
/// A setting that stores a string value.
/// </summary>
/// <seealso cref="MyApp.Features.AppSettings.ISetting" />
public sealed class StringSetting : ISetting
{
    private readonly Func<string, bool> validate;

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSetting"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="parent">The parent category.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="validate">The validation delegate.</param>
    internal StringSetting(string key, Category parent, string defaultValue = "", Func<string, bool>? validate = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"{nameof(key)} cannot be null or empty.", nameof(key));
        }

        ArgumentNullException.ThrowIfNull(parent, nameof(parent));

        this.Key = key;
        this.Parent = parent;
        this.DefaultValue = defaultValue;
        this.value = defaultValue;
        this.validate = validate ?? (_ => true);

        this.Parent.AddSetting(this);
    }

    /// <summary>
    /// Occurs when the <see cref="Value"/> has changed.
    /// </summary>
    public event Action<string>? ValueChanged;

    /// <summary>
    /// Gets the key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public Category Parent { get; }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    public string DefaultValue { get; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value
    {
        get => this.value;
        set
        {
            this.Error = !this.validate(value);
            if (this.value != value && !this.Error)
            {
                this.value = value;
                this.ValueChanged?.Invoke(this.value);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the most recent <c>Value</c> application resulted in a failed validation.
    /// </summary>
    public bool Error { get; private set; }

    /// <inheritdoc />
    object ISetting.DefaultValue => this.DefaultValue;

    /// <inheritdoc />
    object ISetting.Value
    {
        get => this.Value;
        set => this.Value = (string)value;
    }

    /// <inheritdoc />
    string ISetting.Export() => this.Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    void ISetting.Import(string value)
    {
        this.Value = value;
    }
}