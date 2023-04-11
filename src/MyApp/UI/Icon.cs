namespace MyApp.UI;

using System;
using System.Collections.Generic;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Avalonia.Threading;

/// <summary>
/// Displays SVG icons using customizable color.
/// </summary>
/// <seealso cref="Avalonia.Controls.Primitives.TemplatedControl" />
public sealed class Icon : TemplatedControl, IDisposable
{
    /// <summary>
    /// Defines the <see cref="IconName"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> IconNameProperty =
        AvaloniaProperty.Register<Icon, string?>(nameof(IconName));

    /// <summary>
    /// Defines the <see cref="Color"/> property.
    /// </summary>
    public static readonly StyledProperty<IImmutableSolidColorBrush> ColorProperty =
        AvaloniaProperty.Register<Icon, IImmutableSolidColorBrush>(nameof(Color), Brushes.Black);

    /// <summary>
    /// Defines the <see cref="Image"/> property.
    /// </summary>
    public static readonly DirectProperty<Icon, Image> ImageProperty =
        AvaloniaProperty.RegisterDirect<Icon, Image>(
            nameof(Image),
            o => o.Image);

    /// <summary>
    /// The cached SVG strings.
    /// </summary>
    private static readonly Dictionary<string, string?> IconCache = new();

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private readonly SvgSource source;

    /// <summary>
    /// The asset loader.
    /// </summary>
    private readonly IAssetLoader? assetLoader;

    /// <summary>
    /// The back-end field for the corresponding property.
    /// </summary>
    private Image image;

    /// <summary>
    /// Keeps the state of the most recent configuration.
    /// </summary>
    /// <remarks>
    /// Used for batching <see cref="IconName"/> + <see cref="Color"/> bindings together, preventing the Control from being loading SVG multiple times in a row.
    /// </remarks>
    private (string? IconName, IImmutableSolidColorBrush Color) renderInfo;

    /// <summary>
    /// Initializes static members of the <see cref="Icon"/> class.
    /// </summary>
    static Icon()
    {
        IconNameProperty.Changed.AddClassHandler<Icon>((x, e) => x.OnViewChanged(e));
        ColorProperty.Changed.AddClassHandler<Icon>((x, e) => x.OnViewChanged(e));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Icon"/> class.
    /// </summary>
    public Icon()
    {
        this.assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

        this.source = new();
        this.image = new Image { Source = new SvgImage { Source = this.source } };
    }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    /// <remarks>
    /// Should point to a <c>.svg</c> file in <c>/Assets/Icons</c>, without the file extension.
    /// </remarks>
    public string? IconName
    {
        get => this.GetValue(IconNameProperty);
        set => this.SetValue(IconNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    public IImmutableSolidColorBrush Color
    {
        get => this.GetValue(ColorProperty);
        set => this.SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Gets the <see cref="Image"/> element that hosts the <see cref="SvgImage"/>.
    /// </summary>
    public Image Image
    {
        get => this.image;
        private set => this.SetAndRaise(ImageProperty, ref this.image, value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.image.Source = null;
        this.source.Dispose();
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var p1 = this.IconName;
        var p2 = this.Color;

        if (p1 != this.renderInfo.IconName || p2 != this.renderInfo.Color)
        {
            this.renderInfo = new(p1, p2);
            Dispatcher.UIThread.InvokeAsync(() => this.Update(this.renderInfo), DispatcherPriority.Background);
        }

        base.Render(context);
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
    /// Updates the content according to <see cref="IconName"/> and <see cref="Color"/>.
    /// </summary>
    /// <param name="state">The state.</param>
    private void Update((string? IconName, IImmutableSolidColorBrush Color) state)
    {
        if (App.Current is null || this.assetLoader is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(state.IconName))
        {
            this.source.FromSvgDocument(null);
            this.image.InvalidateArrange();
            return;
        }

        if (!IconCache.TryGetValue(state.IconName, out var data))
        {
            var asset = $"avares://{typeof(Program).Assembly.GetName()}/Assets/Icons/{state.IconName}.svg";
            var uri = new Uri(asset);

            if (!this.assetLoader.Exists(uri))
            {
                data = null;
                IconCache.Add(state.IconName, data);
            }
            else
            {
                using var stream = this.assetLoader.Open(uri);
                using var reader = new StreamReader(stream);
                data = reader.ReadToEnd();
                IconCache.Add(state.IconName, data);
            }
        }

        if (string.IsNullOrEmpty(data))
        {
            this.source.FromSvgDocument(null);
            this.image.InvalidateArrange();
            return;
        }

        var color = $"#{this.Color.Color.R:X2}{this.Color.Color.G:X2}{this.Color.Color.B:X2}";
        var colorized = data.Replace("#000000", color);
        this.source.FromSvg(colorized);
        this.image.InvalidateArrange();
    }
}