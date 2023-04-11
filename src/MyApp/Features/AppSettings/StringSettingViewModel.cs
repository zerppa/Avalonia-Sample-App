namespace MyApp.Features.AppSettings;

using System;

/// <summary>
/// The view model for <see cref="StringSetting" />.
/// </summary>
/// <seealso cref="MyApp.Features.AppSettings.SettingViewModel" />
public class StringSettingViewModel : SettingViewModel
{
    private readonly StringSetting model;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSettingViewModel"/> class.
    /// </summary>
    /// <param name="setting">The setting.</param>
    public StringSettingViewModel(StringSetting setting)
    {
        this.model = setting ?? throw new ArgumentNullException(nameof(setting));
    }

    /// <summary>
    /// Gets the key.
    /// </summary>
    public string Key => this.model.Key;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value
    {
        get => this.model.Value;
        set => this.model.Value = value;
    }

    /// <inheritdoc />
    internal override void Refresh()
    {
        this.OnPropertyChanged(nameof(this.Key));
    }
}