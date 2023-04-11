namespace MyApp.Features.HomeScreen;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// View model for a single language item.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public partial class LanguageViewModel : ViewModel
{
    [ObservableProperty]
    private string? localizedName;

    [ObservableProperty]
    private string? icon;

    /// <summary>
    /// Gets the culture identifier. Format is expected to be like <c>en-us</c>.
    /// </summary>
    public required string Identifier { get; init; }
}