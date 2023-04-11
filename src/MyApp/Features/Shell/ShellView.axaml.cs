namespace MyApp.Features.Shell;

using Avalonia.Controls;

/// <summary>
/// The Shell view.
/// </summary>
/// <seealso cref="Avalonia.Controls.UserControl" />
public partial class ShellView : UserControl, IViewContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellView"/> class.
    /// </summary>
    public ShellView()
    {
        this.InitializeComponent();

        App.Current?.Host.Register<IViewContainer>(this);

        this.Loaded += (_, _) =>
        {
            App.Current?.Host.Messenger.Send(new MainViewReadyMessage());
        };
    }

    /// <inheritdoc />
    public void AddOrActivate(Control? control)
    {
        if (control is not null && !this.ViewPanel.Children.Contains(control))
        {
            this.ViewPanel.Children.Add(control);
        }

        foreach (Control child in this.ViewPanel.Children)
        {
            child.IsVisible = child.Equals(control);
        }
    }

    /// <inheritdoc />
    public void Remove(Control? control)
    {
        if (control is not null && this.ViewPanel.Children.Contains(control))
        {
            this.ViewPanel.Children.Remove(control);
        }
    }

    /// <inheritdoc />
    protected override void OnUnloaded()
    {
        App.Current?.Host.Unregister<IViewContainer>(this);

        base.OnUnloaded();
    }
}