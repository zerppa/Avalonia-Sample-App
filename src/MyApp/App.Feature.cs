namespace MyApp;

using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// The application.
/// </summary>
/// <seealso cref="Avalonia.Application" />
/// <seealso cref="MyApp.IHost" />
public partial class App
{
    /// <summary>
    /// The abstract base class for application features. A feature is an autonomous module in the application.
    /// </summary>
    /// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
    public abstract class Feature : ObservableObject
    {
        private IHost? host;

        /// <summary>
        /// Gets or sets the application common services.
        /// </summary>
        public virtual IHost? Host
        {
            get => this.host ??= Current;
            set => this.host = value;
        }

        /// <summary>
        /// Initializes the feature. This is the first call in the initialization sequence,
        /// and will be called (and awaited) for all other features before application moves
        /// to the next step in the initialization sequence.
        ///
        /// You should load any necessary settings and other data here.
        ///
        /// The <see cref="Host"/> has already been set and any dependencies have been resolved.
        /// </summary>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public virtual Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Called after all features have completed their <see cref="InitializeAsync"/>.
        /// Since all features are now initialized, you can establish any communication and
        /// reliance between them.
        ///
        /// You can also start any background services here.
        ///
        /// This step in the sequence is cancellable. If any feature chooses to cancel it,
        /// for example due to incompatibility or missing data or misconfiguration, the entire
        /// application will exit, and <see cref="ShutdownAsync"/> will be called. Do not make
        /// assumptions that <see cref="RunningAsync"/> (the next step) will be called.
        /// </summary>
        /// <param name="cancel">The cancellation token source.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public virtual Task StartingAsync(CancellationTokenSource cancel) => Task.CompletedTask;

        /// <summary>
        /// All features have now been successfully initialized and configured. The main window
        /// will be created and shown.
        ///
        /// You can utilize the <paramref name="args"/> to figure out if the application should
        /// take action.
        /// </summary>
        /// <param name="args">The command line arguments. Does not contain the executable name as the first element.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public virtual Task RunningAsync(string[] args) => Task.CompletedTask;

        /// <summary>
        /// Called when user attempts to close the application.
        ///
        /// This operation is cancellable. If any feature chooses to cancel it, the application
        /// will continue to run.
        ///
        /// You should NOT stop background services, or otherwise prepare to shutdown, as such
        /// actions should be taken in the <see cref="ShutdownAsync"/> method instead.
        ///
        /// You may, for example, show a confirmation dialog to the user, asking whether work
        /// needs to be saved before exiting.
        /// </summary>
        /// <param name="cancel">The cancellation token source.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public virtual Task ExitRequestedAsync(CancellationTokenSource cancel) => Task.CompletedTask;

        /// <summary>
        /// Called when exit had been requested and no feature canceled it. The application
        /// will now shut down, and all features should stop any background tasks and save the
        /// necessary data.
        /// </summary>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public virtual Task ShutdownAsync() => Task.CompletedTask;
    }
}
