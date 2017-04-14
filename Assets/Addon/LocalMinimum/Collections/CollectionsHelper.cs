using System.Collections.Generic;
using System.Linq;

namespace LocalMinimum.Collections
{
    public static class CollectionsHelper
    {

        public static T[] Shuffle<T>(this T[] data, System.Random rndSource)
        {
            List<KeyValuePair<double, T>> list = new List<KeyValuePair<double, T>>();


            for (int i = 0; i < data.Length; i++)
            {
                list.Add(new KeyValuePair<double, T>(rndSource.NextDouble(), data[i]));
            }

            return list.OrderBy(e => e.Key).Select(e => e.Value).ToArray();
        }

        public static List<T> Shuffle<T>(this List<T> data, System.Random rndSource)
        {

            List<KeyValuePair<double, T>> list = new List<KeyValuePair<double, T>>();


            for (int i = 0, l = data.Count; i < l; i++)
            {
                list.Add(new KeyValuePair<double, T>(rndSource.NextDouble(), data[i]));
            }

            return list.OrderBy(e => e.Key).Select(e => e.Value).ToList();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> data, System.Random rndSource)
        {
            T[] shuffeled = data.ToArray().Shuffle(rndSource);
            for (int i=0, l=shuffeled.Length; i< l; i++)
            {
                yield return shuffeled[i];
            }
        }
    }

}
