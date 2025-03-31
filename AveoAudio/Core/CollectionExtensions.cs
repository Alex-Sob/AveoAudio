using System;
using System.Collections.Generic;
using System.Linq;

namespace AveoAudio;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items) collection.Add(item);
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
