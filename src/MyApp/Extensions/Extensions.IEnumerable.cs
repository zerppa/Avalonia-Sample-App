namespace MyApp.Extensions;

using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods.
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Iterates the <see cref="IEnumerable{T}"/> and performs the specified <see cref="Action"/> on each item.
    /// </summary>
    /// <typeparam name="T">Type of items within the <c>IEnumerable</c>.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="action">The action.</param>
    public static void ForEach<T>(this IEnumerable<T>? items, Action<T>? action)
    {
        if (items is null || action is null)
        {
            return;
        }

        foreach (var item in items)
        {
            action(item);
        }
    }
}