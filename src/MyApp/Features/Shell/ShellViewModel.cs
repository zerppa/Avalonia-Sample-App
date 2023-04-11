namespace MyApp.Features.Shell;

using System;

/// <summary>
/// The view model for Shell.
/// </summary>
/// <seealso cref="MyApp.ViewModel" />
public sealed partial class ShellViewModel : ViewModel
{
    private readonly IShell shell;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="shell">The shell.</param>
    public ShellViewModel(IShell shell)
    {
        ArgumentNullException.ThrowIfNull(shell, nameof(shell));

        this.shell = shell;
    }
}