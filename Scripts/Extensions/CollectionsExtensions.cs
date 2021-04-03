using System.Collections.Generic;

namespace Extensions
{
    public static class CollectionsExtensions
    {
        public static T KeyByValue<T, W>(this Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (var pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }

            return key;
        }

        public static List<T> Reverse<T>(this List<T> list)
        {
            list.Reverse();
            return list;
        }
    }
}
