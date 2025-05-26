namespace AveoAudio;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items) collection.Add(item);
    }

    public static T? Random<T>(this IEnumerable<T> source)
    {
        T? result = default;
        int min = int.MaxValue;
        var random = new Random();

        foreach (var item in source)
        {
            var number = random.Next();

            if (number < min)
            {
                result = item;
                min = number;
            }
        }

        return result;
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var random = new Random();

        return from item in source
               let r = random.Next()
               orderby r
               select item;
    }
}
