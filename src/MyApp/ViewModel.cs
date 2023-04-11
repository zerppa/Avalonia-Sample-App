namespace MyApp;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// The view model base class.
/// </summary>
/// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
public abstract class ViewModel : ObservableObject
{
    private IHost? host;

    /// <summary>
    /// Gets or sets the host service provider.
    /// </summary>
    /// <value>The host service provider.</value>
    public IHost? Host
    {
        get => this.host;
        set
        {
            if (this.SetProperty(ref this.host, value) && value is not null)
            {
                this.Initialize();
            }
        }
    }

    /// <summary>
    /// Activates this instance.
    /// If this view model is associated with a Published View, this method will be automatically called when it is attached to the Logical Tree.
    /// </summary>
    public virtual void Activate()
    {
    }

    /// <summary>
    /// Deactivates this instance.
    /// If this view model is associated with a Published View, this method will be automatically called when it is detached from the Logical Tree.
    /// </summary>
    public virtual void Deactivate()
    {
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <remarks>
    /// The <see cref="Host"/> is set.
    /// </remarks>
    protected virtual void Initialize()
    {
    }
}