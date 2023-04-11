namespace MyApp.Features.AppSettings;

using System;

/// <summary>
/// The view model for <see cref="BooleanSetting" />.
/// </summary>
/// <seealso cref="MyApp.Features.AppSettings.SettingViewModel" />
public class BooleanSettingViewModel : SettingViewModel
{
    private readonly BooleanSetting model;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanSettingViewModel"/> class.
    /// </summary>
    /// <param name="setting">The setting.</param>
    public BooleanSettingViewModel(BooleanSetting setting)
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
    public bool Value
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