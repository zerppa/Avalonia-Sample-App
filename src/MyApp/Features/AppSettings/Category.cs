namespace MyApp.Features.AppSettings;

using System;
using System.Collections.ObjectModel;

/// <summary>
/// Container for settings and other categories.
/// </summary>
public class Category
{
    private readonly ObservableCollection<Category> categories = new();
    private readonly ObservableCollection<ISetting> settings = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Category"/> class.
    /// </summary>
    /// <param name="key">The key. Should be unique. Also serves as the localization key.</param>
    /// <param name="parent">The parent category, or <c>null</c> if root category.</param>
    internal Category(string key, Category? parent = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"{nameof(key)} cannot be null or empty.", nameof(key));
        }

        this.Key = key;
        this.Categories = new(this.categories);
        this.Settings = new(this.settings);

        this.Parent = parent;
        this.Parent?.AddCategory(this);
    }

    /// <summary>
    /// Gets the key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <remarks>
    /// Will be <c>null</c> for root categories.
    /// </remarks>
    public Category? Parent { get; private set; }

    /// <summary>
    /// Gets the sub-categories.
    /// </summary>
    public ReadOnlyObservableCollection<Category> Categories { get; }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    public ReadOnlyObservableCollection<ISetting> Settings { get; }

    /// <summary>
    /// Adds the sub-category.
    /// </summary>
    /// <param name="category">The category.</param>
    public void AddCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category, nameof(category));

        if (this.Categories.Contains(category))
        {
            return;
        }

        category.Parent?.RemoveCategory(category);

        this.categories.Add(category);
        category.Parent = this;
    }

    /// <summary>
    /// Adds the setting.
    /// </summary>
    /// <param name="setting">The setting.</param>
    public void AddSetting(ISetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting, nameof(setting));

        if (this.Settings.Contains(setting))
        {
            return;
        }

        this.settings.Add(setting);
    }

    /// <summary>
    /// Removes the sub-category.
    /// </summary>
    /// <param name="category">The category.</param>
    internal void RemoveCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category, nameof(category));

        if (this.categories.Remove(category))
        {
            category.Parent = null;
        }
    }
}