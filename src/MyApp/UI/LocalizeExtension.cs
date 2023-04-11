namespace MyApp.UI;

using System;
using System.Globalization;

using Avalonia.Data;
using Avalonia.Markup.Xaml;

/// <summary>
/// Produces a localized string based on either a <see cref="string"/> key, or <see cref="Binding"/> that supplies a <see cref="string"/>.
/// If no localized string with that key is found, just the key is returned.
///
/// If a binding is supplied, it should NOT use a <c>Converter</c>.
/// </summary>
/// <seealso cref="Avalonia.Markup.Xaml.MarkupExtension" />
public class LocalizeExtension : MarkupExtension
{
    private readonly Func<object> getValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
    /// </summary>
    /// <param name="key">The localization key.</param>
    public LocalizeExtension(string key)
    {
        key ??= string.Empty;
        this.getValue = () => ConvertKeyToLocalizedString.Instance.Convert(key, typeof(string), default, CultureInfo.InvariantCulture) ?? key;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
    /// </summary>
    /// <param name="binding">The binding that produces the localization key.</param>
    public LocalizeExtension(BindingBase binding)
    {
        binding.Converter ??= ConvertKeyToLocalizedString.Instance;

        this.getValue = () => binding;
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.getValue();
    }
}