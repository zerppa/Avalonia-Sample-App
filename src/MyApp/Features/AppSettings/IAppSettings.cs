namespace MyApp.Features.AppSettings;

using System;
using System.Collections.ObjectModel;

/// <summary>
/// Controls the App settings.
/// </summary>
public interface IAppSettings
{
    /// <summary>
    /// Occurs when any setting's value has changed.
    /// </summary>
    event EventHandler<SettingValueChangedEventArgs> ValueChanged;

    /// <summary>
    /// Gets the root categories.
    /// </summary>
    ReadOnlyObservableCollection<Category> Categories { get; }

    /// <summary>
    /// Registers a category and places it in the hierarchy.
    /// If a category with the same key already exists on the specified parent, the existing instance is returned.
    /// Keys are case-sensitive.
    /// </summary>
    /// <param name="key">The key. Should be unique. Also serves as the localization key.</param>
    /// <param name="parent">The parent. If <c>null</c>, the category will be a root category.</param>
    /// <returns>The category.</returns>
    Category RegisterCategory(string key, Category? parent = null);

    /// <summary>
    /// Registers an integer setting and places it in the category specified by <paramref name="categoryPath"/> that contains category keys that form a hierarchical path.
    /// If no category is specified the setting will appear under the default "General" category.
    /// Keys are case-sensitive.
    /// </summary>
    /// <param name="key">The key. Should be unique. Also serves as the localization key.</param>
    /// <param name="categoryPath">List of category keys that form the hierarchy under which the setting will be placed.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="validate">The validation predicate. This delegate will be run every time <see cref="IntegerSetting.Value"/> changes, and updates the <see cref="IntegerSetting.Error"/> property accordingly.</param>
    /// <returns>The setting.</returns>
    IntegerSetting RegisterIntegerSetting(string key, string[] categoryPath, int defaultValue = default, Func<int, bool>? validate = null);

    /// <summary>
    /// Registers a boolean setting and places it in the category specified by <paramref name="categoryPath"/> that contains category keys that form a hierarchical path.
    /// If no category is specified the setting will appear under the default "General" category.
    /// Keys are case-sensitive.
    /// </summary>
    /// <param name="key">The key. Should be unique. Also serves as the localization key.</param>
    /// <param name="categoryPath">List of category keys that form the hierarchy under which the setting will be placed.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="validate">The validation predicate. This delegate will be run every time <see cref="BooleanSetting.Value"/> changes, and updates the <see cref="BooleanSetting.Error"/> property accordingly.</param>
    /// <returns>The setting.</returns>
    BooleanSetting RegisterBooleanSetting(string key, string[] categoryPath, bool defaultValue = default, Func<bool, bool>? validate = null);

    /// <summary>
    /// Registers a string setting and places it in the category specified by <paramref name="categoryPath"/> that contains category keys that form a hierarchical path.
    /// If no category is specified the setting will appear under the default "General" category.
    /// Keys are case-sensitive.
    /// </summary>
    /// <param name="key">The key. Should be unique. Also serves as the localization key.</param>
    /// <param name="categoryPath">List of category keys that form the hierarchy under which the setting will be placed.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="validate">The validation predicate. This delegate will be run every time <see cref="StringSetting.Value"/> changes, and updates the <see cref="StringSetting.Error"/> property accordingly.</param>
    /// <returns>The setting.</returns>
    StringSetting RegisterStringSetting(string key, string[] categoryPath, string defaultValue = "", Func<string, bool>? validate = null);

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    void ResetAll();
}
