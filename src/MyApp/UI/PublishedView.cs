namespace MyApp.UI;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

/// <summary>
/// A special content control that hosts a previously registered Published View.
/// </summary>
/// <seealso cref="Avalonia.Controls.Primitives.TemplatedControl" />
/// <seealso cref="Avalonia.Controls.IContentControl" />
/// <seealso cref="Avalonia.Controls.Presenters.IContentPresenterHost" />
[TemplatePart("PART_ContentPresenter", typeof(IContentPresenter))]
public class PublishedView : TemplatedControl, IContentControl, IContentPresenterHost
{
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<PublishedView, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<PublishedView, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Defines the <see cref="HorizontalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<PublishedView, HorizontalAlignment>(nameof(HorizontalContentAlignment));

    /// <summary>
    /// Defines the <see cref="VerticalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<PublishedView, VerticalAlignment>(nameof(VerticalContentAlignment));

    /// <summary>
    /// Defines the <see cref="ViewName"/> property.
    /// </summary>
    public static readonly DirectProperty<PublishedView, string?> ViewNameProperty =
        AvaloniaProperty.RegisterDirect<PublishedView, string?>(
            nameof(ViewName),
            o => o.ViewName,
            (o, v) => o.ViewName = v);

    /// <summary>
    /// Defines the <see cref="ViewParameter"/> property.
    /// </summary>
    public static readonly DirectProperty<PublishedView, object?> ViewParameterProperty =
        AvaloniaProperty.RegisterDirect<PublishedView, object?>(
            nameof(ViewParameter),
            o => o.ViewParameter,
            (o, v) => o.ViewParameter = v);

    /// <summary>
    /// Defines the <see cref="DefaultContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        AvaloniaProperty.Register<PublishedView, object?>(nameof(DefaultContent), defaultBindingMode: BindingMode.OneWayToSource);

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private string? viewName;

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private object? viewParameter;

    /// <summary>
    /// Keeps the state of the most recent configuration.
    /// </summary>
    /// <remarks>
    /// Used for batching <see cref="ViewName"/> + <see cref="ViewParameter"/> + <see cref="DefaultContent"/> bindings together, preventing the View from being instantiated multiple times in a row.
    /// </remarks>
    private (string? ViewName, object? ViewParameter, object? DefaultContent) renderInfo;

    /// <summary>
    /// Initializes static members of the <see cref="PublishedView"/> class.
    /// </summary>
    static PublishedView()
    {
        ContentProperty.Changed.AddClassHandler<PublishedView>((x, e) => x.OnContentChanged(e));
        ViewNameProperty.Changed.AddClassHandler<PublishedView>((x, e) => x.OnViewChanged(e));
        ViewParameterProperty.Changed.AddClassHandler<PublishedView>((x, e) => x.OnViewChanged(e));
        DefaultContentProperty.Changed.AddClassHandler<PublishedView>((x, e) => x.OnViewChanged(e));
    }

    /// <summary>
    /// Gets or sets the view name.
    /// </summary>
    public string? ViewName
    {
        get => this.viewName;
        set => this.SetAndRaise(ViewNameProperty, ref this.viewName, value);
    }

    /// <summary>
    /// Gets or sets the view parameter.
    /// </summary>
    public object? ViewParameter
    {
        get => this.viewParameter;
        set => this.SetAndRaise(ViewParameterProperty, ref this.viewParameter, value);
    }

    /// <summary>
    /// Sets the default content that is shown if the Published View is not resolved.
    /// </summary>
    public object? DefaultContent
    {
        private get => this.GetValue(DefaultContentProperty);
        set => this.SetValue(DefaultContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the content to display.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => this.GetValue(ContentProperty);
        set => this.SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the content of the control.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => this.GetValue(ContentTemplateProperty);
        set => this.SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets the presenter from the control's template.
    /// </summary>
    public IContentPresenter? Presenter { get; private set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of the content within the control.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => this.GetValue(HorizontalContentAlignmentProperty);
        set => this.SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the content within the control.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => this.GetValue(VerticalContentAlignmentProperty);
        set => this.SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <inheritdoc/>
    IAvaloniaList<ILogical> IContentPresenterHost.LogicalChildren => this.LogicalChildren;

    /// <inheritdoc/>
    bool IContentPresenterHost.RegisterContentPresenter(IContentPresenter presenter)
    {
        return this.RegisterContentPresenter(presenter);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var p1 = this.ViewName;
        var p2 = this.ViewParameter;
        var p3 = this.DefaultContent;

        if (p1 != this.renderInfo.ViewName || p2 != this.renderInfo.ViewParameter || p3 != this.renderInfo.DefaultContent)
        {
            this.renderInfo = new(p1, p2, p3);
            Dispatcher.UIThread.InvokeAsync(() => this.Update(this.renderInfo), DispatcherPriority.Background);
        }

        base.Render(context);
    }

    /// <summary>
    /// Called when an <see cref="IContentPresenter" /> is registered with the control.
    /// </summary>
    /// <param name="presenter">The presenter.</param>
    /// <returns>
    /// True if the content presenter should add its child to the logical children of the
    /// host; otherwise false.
    /// </returns>
    protected virtual bool RegisterContentPresenter(IContentPresenter presenter)
    {
        if (presenter.Name == "PART_ContentPresenter")
        {
            this.Presenter = presenter;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Called when the <see cref="ContentProperty"/> changed.
    /// </summary>
    /// <param name="e">The <see cref="AvaloniaPropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        this.UpdateLogicalTree(e.OldValue, e.NewValue);
    }

    /// <summary>
    /// Updates the logical tree.
    /// </summary>
    /// <param name="toRemove">The object to remove.</param>
    /// <param name="toAdd">The object to add.</param>
    protected void UpdateLogicalTree(object? toRemove, object? toAdd)
    {
        if (toRemove is ILogical oldChild)
        {
            this.LogicalChildren.Remove(oldChild);
        }

        if (toAdd is ILogical newChild)
        {
            this.LogicalChildren.Add(newChild);
        }
    }

    /// <summary>
    /// Called when a property that affects what content should be shown has changed.
    /// </summary>
    /// <param name="e">The <see cref="AvaloniaPropertyChangedEventArgs"/> instance containing the event data.</param>
    private void OnViewChanged(AvaloniaPropertyChangedEventArgs e)
    {
        this.InvalidateVisual();
    }

    /// <summary>
    /// Updates the content according to <see cref="ViewName"/>, <see cref="ViewParameter"/> and <see cref="DefaultContent"/>.
    /// </summary>
    /// <param name="state">The state.</param>
    private void Update((string? ViewName, object? ViewParameter, object? DefaultContent) state)
    {
        // Debug.WriteLine($"{state.ViewName}, {state.ViewParameter}, {state.DefaultContent}");
        if (App.Current is not null && state.ViewName is not null)
        {
            this.Content = App.Current.CreateView(state.ViewName, state.ViewParameter) ?? this.DefaultContent;
        }
        else
        {
            this.Content = this.DefaultContent;
        }
    }
}