namespace MyApp.UI;

using Avalonia;
using Avalonia.Data;

/// <summary>
/// Enables binding to an element's class list.
/// </summary>
public class BindableStyleClasses
{
    /// <summary>
    /// Defines the <see cref="Classes"/> property.
    /// </summary>
    public static readonly AttachedProperty<string> ClassesProperty =
        AvaloniaProperty.RegisterAttached<BindableStyleClasses, StyledElement, string>(
            "Classes",
            string.Empty,
            false,
            BindingMode.OneTime);

    /// <summary>
    /// Initializes static members of the <see cref="BindableStyleClasses"/> class.
    /// </summary>
    static BindableStyleClasses()
    {
        ClassesProperty.Changed.AddClassHandler<StyledElement>(OnClassesPropertyChanged);
    }

    /// <summary>
    /// Sets the classes.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="value">The value.</param>
    public static void SetClasses(AvaloniaObject element, string value)
    {
        element.SetValue(ClassesProperty, value);
    }

    /// <summary>
    /// Gets the classes.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The value.</returns>
    public static string GetClasses(AvaloniaObject element)
    {
        return element.GetValue(ClassesProperty);
    }

    /// <summary>
    /// Called when the "Classes" attached property value changes.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="e">The <see cref="AvaloniaPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnClassesPropertyChanged(StyledElement element, AvaloniaPropertyChangedEventArgs e)
    {
        if (element is not null)
        {
            element.Classes.Clear();
            element.Classes.AddRange((e.NewValue as string ?? string.Empty).Split(' ', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries));
        }
    }
}
