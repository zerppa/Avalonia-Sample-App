namespace MyApp;

using System;

using Avalonia;

/// <summary>
/// Avalonia Sample App.
/// </summary>
internal class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <remarks>
    /// Do not use any Avalonia, third-party APIs or any SynchronizationContext-reliant code before AppMain is called.
    /// </remarks>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnExplicitShutdown);


    /// <summary>
    /// Builds the Avalonia application.
    /// </summary>
    /// <returns>The enriched <see cref="AppBuilder"/>.</returns>
    /// <remarks>
    /// Avalonia configuration. Do not remove, also used by visual designer.
    /// </remarks>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
}