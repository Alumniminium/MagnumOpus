namespace MagnumOpus.Helpers;

/// <summary>
/// Extension methods for integer operations and array counting utilities.
/// Provides specialized counting algorithms for collection processing.
/// </summary>
public static class IntegerExt
{
    /// <summary>
    /// Counts consecutive elements from the start of an array that match the given predicate.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection</typeparam>
    /// <param name="collection">Array to count elements from</param>
    /// <param name="match">Predicate to test elements against</param>
    /// <returns>Number of consecutive matching elements from the start</returns>
    public static int CountWhile<T>(this T[] collection, Predicate<T> match)
    {
        for (var I = 0; I < collection.Length; I++)
            if (!match(collection[I]))
                return I;
        return collection.Length;
    }
}