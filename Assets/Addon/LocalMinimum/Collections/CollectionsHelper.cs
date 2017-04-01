using System.Collections.Generic;
using System.Linq;

namespace LocalMinimum.Collections
{
    public static class CollectionsHelper
    {

        public static T[] Shuffle<T>(this T[] data)
        {
            List<KeyValuePair<double, T>> list = new List<KeyValuePair<double, T>>();


            for (int i = 0; i < data.Length; i++)
            {
                list.Add(new KeyValuePair<double, T>(PlayerRunData.stats.lvlRnd.NextDouble(), data[i]));
            }

            return list.OrderBy(e => e.Key).Select(e => e.Value).ToArray();
        }

        public static List<T> Shuffle<T>(this List<T> data)
        {

            List<KeyValuePair<double, T>> list = new List<KeyValuePair<double, T>>();


            for (int i = 0, l = data.Count; i < l; i++)
            {
                list.Add(new KeyValuePair<double, T>(PlayerRunData.stats.lvlRnd.NextDouble(), data[i]));
            }

            return list.OrderBy(e => e.Key).Select(e => e.Value).ToList();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> data)
        {
            T[] shuffeled = data.ToArray().Shuffle();
            for (int i=0, l=shuffeled.Length; i< l; i++)
            {
                yield return shuffeled[i];
            }
        }
    }

}
