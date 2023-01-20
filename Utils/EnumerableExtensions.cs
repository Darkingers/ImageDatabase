namespace Utils
{
    public static class EnumerableExtensions
    {
        //A generic method that returns true if an enumerable contains all the element of another enumerable.
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                if (!source.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}