namespace MyApp.Features.AppSettings;

using System;

/// <summary>
/// Holds the event arguments for the <see cref="IAppSettings.ValueChanged"/> event.
/// </summary>
/// <seealso cref="System.EventArgs" />
public class SettingValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingValueChangedEventArgs"/> class.
    /// </summary>
    /// <param name="settingKey">The setting key.</param>
    /// <param name="newValue">The new value.</param>
    public SettingValueChangedEventArgs(string settingKey, object? newValue)
    {
        this.SettingKey = settingKey;
        this.NewValue = newValue;
    }

    /// <summary>
    /// Gets the setting key.
    /// </summary>
    public string SettingKey { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; }
}