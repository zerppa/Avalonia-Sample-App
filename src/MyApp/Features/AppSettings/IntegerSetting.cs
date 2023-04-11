namespace MyApp.Features.AppSettings;

using System;
using System.Diagnostics;
using System.Globalization;

/// <summary>
/// A setting that stores a 32-bit integer value.
/// </summary>
/// <seealso cref="MyApp.Features.AppSettings.ISetting" />
public sealed class IntegerSetting : ISetting
{
    private readonly Func<int, bool> validate;

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private int value;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerSetting"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="parent">The parent category.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="validate">The validation delegate.</param>
    internal IntegerSetting(string key, Category parent, int defaultValue = default, Func<int, bool>? validate = null)
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
    public event Action<int>? ValueChanged;

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
    public int DefaultValue { get; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public int Value
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
        set => this.Value = (int)value;
    }

    /// <inheritdoc />
    string ISetting.Export() => this.Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    void ISetting.Import(string value)
    {
        if (int.TryParse(value ?? string.Empty, CultureInfo.InvariantCulture, out var val))
        {
            this.Value = val;
        }
        else
        {
            Trace.TraceWarning($"Could not import setting '{this.Key}' value because of invalid input '{value}'.");
            this.Value = this.DefaultValue;
        }
    }
}