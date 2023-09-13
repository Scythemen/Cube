using System.Collections.Generic;

namespace Cube.Utility
{
    public static class RedisHelper
    {
        /// <summary>
        /// convert a k-v sequence array to dictionary.<br/>
        /// e.g: arr ["k1" "v1" "k2" "v2" "k3" "v3"] -> dic {"k1":"v1","k2":"v2","k3":"v3"}
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Dictionary<string, string> RedisKVToDictionary(this object array)
        {
            if (array is not object[] a) return null;
            var d = new Dictionary<string, string>();
            for (int i = 0; i < a.Length; i = i + 2)
            {
                if (!string.IsNullOrWhiteSpace(a[i]?.ToString()))
                {
                    d.Add(a[i].ToString(), a[i + 1]?.ToString());
                }
            }

            return d;
        }

        /// <summary>
        /// convert a dictionary to k-v sequence array.<br/>
        /// e.g: dic {"k1":"v1","k2":"v2","k3":"v3"} -> arr ["k1" "v1" "k2" "v2" "k3" "v3"] 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string[] ToRedisKV(this Dictionary<string, string> dic)
        {
            var list = new List<string>();
            foreach (var item in dic)
            {
                if (!string.IsNullOrWhiteSpace(item.Value))
                {
                    list.Add(item.Key);
                    list.Add(item.Value);
                }
            }

            return list.ToArray();
        }
    }
}