namespace MyApp.Features.ViewSelector;

using System.Collections;
using System.Collections.Specialized;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;

/// <summary>
/// The View Selector view.
/// </summary>
/// <seealso cref="Avalonia.Controls.UserControl" />
public partial class ViewSelectorView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewSelectorView"/> class.
    /// </summary>
    public ViewSelectorView()
    {
        this.InitializeComponent();

        ItemsControl.ItemsProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<IEnumerable?>>(this.OnItemsChanged));

        var viewSelector = App.Current?.Host.GetInstance<IViewSelector>();
        if (viewSelector is not null)
        {
            // NOTE: This control is expected to be created only once, so no need to unsubscribe.
            viewSelector.ViewChanged += this.OnViewChanged;
        }
    }

    /// <summary>
    /// Called when the currently active view changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ViewChangedEventArgs"/> instance containing the event data.</param>
    private void OnViewChanged(object? sender, ViewChangedEventArgs e)
    {
        // If the view is a project view, scroll it into view
        if (e.To is ProjectItem projectItem)
        {
            var i = 0;
            foreach (var item in (this.ItemsContainer.Items ?? Enumerable.Empty<object>()).OfType<ProjectItemViewModel>())
            {
                if (item.Match(projectItem))
                {
                    if (this.ItemsContainer.ContainerFromIndex(i) is { } element)
                    {
                        Dispatcher.UIThread.InvokeAsync(element.BringIntoView);
                    }

                    break;
                }

                i++;
            }
        }
    }

    /// <summary>
    /// Called when the <c>Items</c> property has changed.
    /// </summary>
    /// <param name="obj">The <see cref="AvaloniaPropertyChangedEventArgs{IEnumerable}"/> instance containing the event data.</param>
    private void OnItemsChanged(AvaloniaPropertyChangedEventArgs<IEnumerable?> obj)
    {
        if (obj.OldValue.Value is INotifyCollectionChanged oldValue)
        {
            oldValue.CollectionChanged -= this.OnCollectionChanged;
        }

        if (obj.NewValue.Value is INotifyCollectionChanged newValue)
        {
            newValue.CollectionChanged += this.OnCollectionChanged;
        }
    }

    /// <summary>
    /// Called when the views collection has changed. Ensures that the layout is updated accordingly.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.OnSizeChangedAsync(null, new SizeChangedEventArgs(null));
    }

    /// <summary>
    /// Called when the control's rendering size has changed. Ensures that the layout is updated accordingly.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        this.ListActionsContainer.Height = this.CombinedContainer.Bounds.Height - this.ItemsContainer.Bounds.Height;
    }

    /// <summary>
    /// Called when the control's rendering size has changed. Ensures that the layout is updated accordingly.
    /// This version delegates the call to UI dispatcher, and the action will take place on next render.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
    private async void OnSizeChangedAsync(object? sender, SizeChangedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => this.ListActionsContainer.Height = this.CombinedContainer.Bounds.Height - this.ItemsContainer.Bounds.Height);
    }

    /// <summary>
    /// Called when the <see cref="UserControl.Loaded"/> event is fired.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (App.Current?.MainWindow is { } window)
        {
            window.SizeChanged += this.OnSizeChangedAsync;
        }

        this.SizeChanged += this.OnSizeChanged;

        this.OnSizeChangedAsync(this, new SizeChangedEventArgs(null));
    }

    /// <summary>
    /// Called when the <see cref="UserControl.Unloaded"/> event is fired.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (App.Current?.MainWindow is { } window)
        {
            window.SizeChanged -= this.OnSizeChangedAsync;
        }

        this.SizeChanged -= this.OnSizeChanged;
    }
}