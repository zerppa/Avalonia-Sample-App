namespace MyApp.Features.AppSettings;

using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

/// <summary>
/// The app settings view.
/// </summary>
/// <seealso cref="Avalonia.Controls.UserControl" />
public partial class AppSettingsView : UserControl
{
    /// <summary>
    /// Defines the <see cref="ScrollIntoView"/> property.
    /// </summary>
    public static readonly DirectProperty<AppSettingsView, object?> ScrollIntoViewProperty =
        AvaloniaProperty.RegisterDirect<AppSettingsView, object?>(
            nameof(ScrollIntoView),
            o => o.ScrollIntoView,
            (o, v) => o.ScrollIntoView = v);

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private object? scrollIntoView;

    /// <summary>
    /// Initializes static members of the <see cref="AppSettingsView"/> class.
    /// </summary>
    static AppSettingsView()
    {
        ScrollIntoViewProperty.Changed.AddClassHandler<AppSettingsView>((x, e) => x.HandleScrollIntoView(e.NewValue));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsView"/> class.
    /// </summary>
    public AppSettingsView()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the scroll into view object instance.
    /// </summary>
    public object? ScrollIntoView
    {
        get => this.scrollIntoView;
        set => this.SetAndRaise(ScrollIntoViewProperty, ref this.scrollIntoView, value);
    }

    /// <summary>
    /// Handles the scroll into view value change.
    /// </summary>
    /// <param name="item">The target DataContext item.</param>
    private void HandleScrollIntoView(object? item)
    {
        if (item is null)
        {
            return;
        }

        var contentPresenter = this.SettingItemsRootContainer.GetVisualDescendants().FirstOrDefault(control => control is ContentPresenter cp && cp.DataContext == item) as Control;
        contentPresenter?.FindDescendantOfType<Panel>()?.BringIntoView();
    }
}