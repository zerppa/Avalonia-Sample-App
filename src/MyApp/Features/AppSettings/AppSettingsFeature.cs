namespace MyApp.Features.AppSettings;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Threading;

/// <summary>
/// The application settings.
/// </summary>
/// <seealso cref="MyApp.Features.AppSettings.IAppSettings" />
/// <seealso cref="MyApp.App.Feature" />
[LocalizedResources(Culture = "en-us", Path = "Locales/en-us.axaml")]
[LocalizedResources(Culture = "fi-fi", Path = "Locales/fi-fi.axaml")]
public class AppSettingsFeature : App.Feature, IAppSettings
{
    private readonly ObservableCollection<Category> categories = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsFeature"/> class.
    /// </summary>
    public AppSettingsFeature()
    {
        this.Categories = new ReadOnlyObservableCollection<Category>(this.categories);
    }

    /// <inheritdoc />
    public event EventHandler<SettingValueChangedEventArgs>? ValueChanged;

    /// <inheritdoc />
    public ReadOnlyObservableCollection<Category> Categories { get; }

    /// <summary>
    /// Creates the Settings view.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view model.</returns>
    [PublishedView<AppSettingsView>("IDE.AppSettings", Description = "The App Settings view.")]
    public ViewModel? CreateAppSettings(object? parameter)
    {
        return new AppSettingsViewModel(this);
    }

    /// <inheritdoc />
    public Category RegisterCategory(string key, Category? parent = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"The {nameof(key)} cannot be null or empty.");
        }

        var existing = this.AllCategories().FirstOrDefault(c => c.Key == key && c.Parent == parent);
        if (existing is not null)
        {
            return existing;
        }

        var category = new Category(key, parent);
        if (parent is null)
        {
            this.categories.Add(category);
        }

        return category;
    }

    /// <inheritdoc />
    public IntegerSetting RegisterIntegerSetting(string key, string[] categoryPath, int defaultValue = default, Func<int, bool>? validate = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"The {nameof(key)} cannot be null or empty.");
        }

        var parent = this.GetOrCreateParent(categoryPath);
        var setting = new IntegerSetting(
            key,
            parent,
            defaultValue,
            validate);

        setting.ValueChanged += x => this.ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(key, x));

        return setting;
    }

    /// <inheritdoc />
    public BooleanSetting RegisterBooleanSetting(string key, string[] categoryPath, bool defaultValue = default, Func<bool, bool>? validate = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"The {nameof(key)} cannot be null or empty.");
        }

        var parent = this.GetOrCreateParent(categoryPath);
        var setting = new BooleanSetting(
            key,
            parent,
            defaultValue,
            validate);

        setting.ValueChanged += x => this.ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(key, x));

        return setting;
    }

    /// <inheritdoc />
    public StringSetting RegisterStringSetting(string key, string[] categoryPath, string defaultValue = "", Func<string, bool>? validate = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"The {nameof(key)} cannot be null or empty.");
        }

        var parent = this.GetOrCreateParent(categoryPath);
        var setting = new StringSetting(
            key,
            parent,
            defaultValue,
            validate);

        setting.ValueChanged += x => this.ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(key, x));

        return setting;
    }

    /// <inheritdoc />
    public void ResetAll()
    {
        foreach (var setting in this.AllSettings())
        {
            setting.Value = setting.DefaultValue;
        }
    }

    /// <inheritdoc />
    public override async Task InitializeAsync()
    {
        // Create the default structure for the setting view
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var general = this.RegisterCategory(CommonCategories.General);
            var test1 = this.RegisterCategory(CommonCategories.Test1);
            var test1sub1 = this.RegisterCategory(CommonCategories.Test1Sub1, test1);
            var test2 = this.RegisterCategory(CommonCategories.Test2);
        });
    }

    /// <inheritdoc />
    public override async Task StartingAsync(CancellationTokenSource cancel)
    {
        // Note: Features should add their Settings during the StartingAsync method!
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var myExampleSetting = this.RegisterIntegerSetting("some_integer_setting", new[] { CommonCategories.General });
        });
    }

    /// <summary>
    /// Finds the category by key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The found category, or <c>null</c> if not found.</returns>
    internal Category? FindCategory(string key)
    {
        return this.AllCategories().FirstOrDefault(c => c.Key == key);
    }

    /// <summary>
    /// Ensures that a parent chain exists.
    /// </summary>
    /// <param name="categoryPath">The category path.</param>
    /// <returns>The most specific category within the chain.</returns>
    private Category GetOrCreateParent(string[] categoryPath)
    {
        var path = categoryPath is { Length: > 0 } ? categoryPath : new[] { CommonCategories.General };

        Category? parent = null;
        foreach (var part in path)
        {
            var asKey = string.IsNullOrWhiteSpace(part) ? "." : part;

            if (parent is null)
            {
                parent = this.Categories.FirstOrDefault(c => c.Key == asKey) ?? AddAsRoot(new Category(asKey));
            }
            else
            {
                parent = parent.Categories.FirstOrDefault(c => c.Key == asKey) ?? new Category(asKey, parent);
            }
        }

        return parent!;

        Category AddAsRoot(Category category)
        {
            this.categories.Add(category);
            return category;
        }
    }

    /// <summary>
    /// Enumerates all categories recursively.
    /// </summary>
    /// <returns>The categories.</returns>
    private IEnumerable<Category> AllCategories()
    {
        var results = new List<Category>();

        Iterate(this.Categories, results);

        return results;

        static void Iterate(IEnumerable<Category> categories, ICollection<Category> results)
        {
            foreach (var category in categories)
            {
                results.Add(category);
                Iterate(category.Categories, results);
            }
        }
    }

    /// <summary>
    /// Enumerates all settings recursively from all categories.
    /// </summary>
    /// <returns>The settings.</returns>
    private IEnumerable<ISetting> AllSettings()
    {
        var results = new List<ISetting>();

        foreach (var category in this.Categories)
        {
            IterateCategory(category, results);
        }

        return results.Distinct();

        static void IterateCategory(Category category, ICollection<ISetting> results)
        {
            foreach (var setting in category.Settings)
            {
                results.Add(setting);
            }

            foreach (var subCategory in category.Categories)
            {
                IterateCategory(subCategory, results);
            }
        }
    }
}
