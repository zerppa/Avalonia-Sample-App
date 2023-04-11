namespace MyApp.Features.AppSettings;

/// <summary>
/// Base class for setting view models.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public abstract class SettingViewModel : ViewModel
{
    /// <summary>
    /// Refreshes localizable bindings.
    /// </summary>
    internal virtual void Refresh()
    {
    }
}