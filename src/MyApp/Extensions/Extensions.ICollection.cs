namespace MyApp.Extensions;

using System.Collections.Generic;

/// <summary>
/// Extension methods.
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Individually adds each element from a <see cref="IEnumerable{T}"/> to the specified collection.
    /// </summary>
    /// <typeparam name="T">Type of items within the <c>IEnumerable</c> and collection.</typeparam>
    /// <param name="collection">The target collection.</param>
    /// <param name="items">The items to be added.</param>
    public static void AddRange<T>(this ICollection<T>? collection, IEnumerable<T>? items)
    {
        if (collection is null || items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}