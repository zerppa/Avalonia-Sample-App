namespace MyApp;

using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

/// <summary>
/// The default implicit data template.
/// Attempts to match a view model object to a view based on a naming convention.
/// The pattern is MyViewModel -> MyView.
/// </summary>
/// <seealso cref="Avalonia.Controls.Templates.IDataTemplate" />
public class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public Control Build(object? param)
    {
        var name = param?.GetType().FullName!.Replace("ViewModel", "View");
        var type = name is not null ? Type.GetType(name) : null;

        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <inheritdoc />
    public bool Match(object? data)
    {
        return data is ViewModel;
    }
}