namespace MyApp.Features.AppSettings;

using System;
using System.Collections.ObjectModel;

using MyApp.Extensions;

/// <summary>
/// The view model for <see cref="Category"/>.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public class CategoryViewModel : ViewModel
{
    private readonly Category model;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryViewModel"/> class.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <exception cref="System.ArgumentNullException">category</exception>
    public CategoryViewModel(Category category)
    {
        this.model = category ?? throw new ArgumentNullException(nameof(category));

        var level = 1;
        var iterator = category.Parent;
        while (iterator is not null)
        {
            level++;
            iterator = iterator.Parent;
        }

        this.Level = level;

        foreach (ISetting setting in category.Settings)
        {
            this.Settings.Add(setting switch
            {
                IntegerSetting integer => new IntegerSettingViewModel(integer),
                BooleanSetting boolean => new BooleanSettingViewModel(boolean),
                StringSetting @string => new StringSettingViewModel(@string),
                _ => new UnknownSettingViewModel(setting),
            });
        }

        foreach (Category subCategory in category.Categories)
        {
            this.Categories.Add(new CategoryViewModel(subCategory));
        }

        /*
         * TODO:
         * Subscribe to Categories + Settings and sync the two collections.
         * The best practice is to declare categories and setting in a Feature's StartingAsync method,
         * but it's of course possible for the Feature to make changes also after that point.
         * Without collection syncing the new changes won't be reflected on the UI.
         */
    }

    /// <summary>
    /// Gets the categories.
    /// </summary>
    public ObservableCollection<CategoryViewModel> Categories { get; } = new();

    /// <summary>
    /// Gets the settings.
    /// </summary>
    public ObservableCollection<SettingViewModel> Settings { get; } = new();

    /// <summary>
    /// Gets the key.
    /// </summary>
    public string Key => this.model.Key;

    /// <summary>
    /// Gets the hierarchy level within the category tree.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Determines whether the specified <paramref name="item"/> matches the underlying model instance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <remarks>Instead of exposing the model instance, we provide a test for equals.</remarks>
    /// <returns><c>true</c> if the test succeeds.</returns>
    internal bool Match(Category? item) => this.model == item;

    /// <summary>
    /// Recursively refreshes bindings to the <see cref="Key"/>.
    /// </summary>
    internal void Refresh()
    {
        this.OnPropertyChanged(nameof(this.Key));

        this.Settings.ForEach(s => s.Refresh());
        this.Categories.ForEach(c => c.Refresh());
    }
}