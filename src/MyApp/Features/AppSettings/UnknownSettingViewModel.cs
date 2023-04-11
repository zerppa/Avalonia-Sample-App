namespace MyApp.Features.AppSettings;

using System;

/// <summary>
/// The view model for general <see cref="ISetting" />.
/// </summary>
/// <seealso cref="MyApp.Features.AppSettings.SettingViewModel" />
public class UnknownSettingViewModel : SettingViewModel
{
    private readonly ISetting model;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownSettingViewModel"/> class.
    /// </summary>
    /// <param name="setting">The setting.</param>
    public UnknownSettingViewModel(ISetting setting)
    {
        this.model = setting ?? throw new ArgumentNullException(nameof(setting));
    }
}