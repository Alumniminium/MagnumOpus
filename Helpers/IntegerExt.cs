namespace MagnumOpus.Helpers
{
    public static class IntegerExt
    {
        public static int CountWhile<T>(this T[] collection, Predicate<T> match)
        {
            for (var I = 0; I < collection.Length; I++)
                if (!match(collection[I]))
                    return I;
            return collection.Length;
        }
    }
}