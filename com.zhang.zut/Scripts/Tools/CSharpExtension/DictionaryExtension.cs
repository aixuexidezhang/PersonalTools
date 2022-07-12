using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools.CSharpExtension
{
    public static class DictionaryExtension
    {

        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> d, Action<TKey, TValue> action)
        {
            if (action == null)
            {
                throw new Exception("需传入 action 参数");
            }
            foreach (var kv in d)
            {
                action(kv.Key, kv.Value);
            }
        }

        public static void ForEachSafe<TKey, TValue>(this Dictionary<TKey, TValue> d, Action<TKey, TValue> action)
        {
            if (action == null)
            {
                throw new Exception("需传入 action 参数");
            }
            var list = d.GetKeyList();
            for (int i = list.Count - 1; i >= 0; i -= 1)
            {
                var key = list[i];
                action(key, d[key]);
            }
        }

        public static KeyValuePair<TKey, TValue> FindKV<TKey, TValue>(this Dictionary<TKey, TValue> d, Predicate<TKey> match)
        {
            if (match == null)
            {
                throw new Exception("需传入 match 参数");
            }
            foreach (var kv in d)
            {
                if (match(kv.Key))
                {
                    return kv;
                }
            }
            return default;
        }

        public static KeyValuePair<TKey, TValue> FindKV<TKey, TValue>(this Dictionary<TKey, TValue> d, Predicate<TValue> match)
        {
            if (match == null)
            {
                throw new Exception("需传入 match 参数");
            }
            foreach (var kv in d)
            {
                if (match(kv.Value))
                {
                    return kv;
                }
            }
            return default;
        }

        public static TKey FindKey<TKey, TValue>(this Dictionary<TKey, TValue> d, Predicate<TKey> match)
        {
            if (match == null)
            {
                throw new Exception("需传入 match 参数");
            }
            foreach (var kv in d)
            {
                if (match(kv.Key))
                {
                    return kv.Key;
                }
            }
            return default;
        }

        public static TValue FindValue<Tkey, TValue>(this Dictionary<Tkey, TValue> d, Predicate<TValue> match)
        {
            if (match == null)
            {
                throw new Exception("需传入 match 参数");
            }
            foreach (var kv in d)
            {
                if (match(kv.Value))
                {
                    return kv.Value;
                }
            }
            return default;
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue defaultValue = default)
        {
            if (key == null)
            {
                return defaultValue;
            }
            if (d.ContainsKey(key))
            {
                return d[key];
            }
            else
            {
                return defaultValue;
            }
        }

        public static TValue GetValue<TKey, TValue>(this SortedDictionary<TKey, TValue> d, TKey key, TValue value = default)
        {
            return d.ContainsKey(key) ? d[key] : value;
        }

        public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key)
        {
            if (key == null)
            {
                return false;
            }
            if (d.ContainsKey(key))
            {
                d.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryRemove<TKey, TValue>(this SortedDictionary<TKey, TValue> d, TKey key)
        {
            if (d.ContainsKey(key))
            {
                d.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Dictionary<TKey, TValue> AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (!d.ContainsKey(key))
            {
                d.Add(key, value);
            }
            return d;
        }

        public static Dictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (d.ContainsKey(key))
            {
                d[key] = value;
            }
            else
            {
                d.Add(key, value);
            }
            return d;
        }

        public static SortedDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this SortedDictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (d.ContainsKey(key))
            {
                d[key] = value;
            }
            else
            {
                d.Add(key, value);
            }
            return d;
        }

        public static List<TKey> GetKeyList<TKey, TValue>(this Dictionary<TKey, TValue> dic)
        {
            return new List<TKey>(dic.Keys);
        }

        public static List<TValue> GetValueList<TKey, TValue>(this Dictionary<TKey, TValue> d)
        {
            return new List<TValue>(d.Values);
        }

        public static List<TValue> GetValueList<TKey, TValue>(this SortedDictionary<TKey, TValue> d)
        {
            return new List<TValue>(d.Values);
        }

        public static TKey GetKeyWithValue<TKey, TValue>(this Dictionary<TKey, TValue> d, TValue value)
        {
            foreach (var kv in d)
            {
                if (kv.Value.Equals(value))
                {
                    return kv.Key;
                }
            }
            return default;
        }

        public static TKey GetKeyWithValue<TKey, TValue>(this SortedDictionary<TKey, TValue> d, TValue value)
        {
            foreach (var kv in d)
            {
                if (kv.Value.Equals(value))
                {
                    return kv.Key;
                }
            }
            return default;
        }

        public static string ToJsonString<Tkey, TValue>(this Dictionary<Tkey, TValue> dic)
        {
            string s = "{";
            int c = 0;
            foreach (KeyValuePair<Tkey, TValue> kv in dic)
            {
                s += kv.Key.ToString() + ":";
                s += kv.Value.ToString();
                if (c < dic.Count - 1)
                {
                    s += ",";
                }
                c += 1;
            }
            s += "}";
            return s;
        }

        public static string ToCsvString<Tkey, TValue>(this Dictionary<Tkey, TValue> dic)
        {
            string s = "";
            int c = 0;

            foreach (KeyValuePair<Tkey, TValue> kv in dic)
            {

                FieldInfo[] fields = kv.Value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

                if (c == 0)
                { // 标题
                    s += kv.Key.GetType().Name + ",";
                    for (int i = 0; i < fields.Length; i++)
                    {
                        s += fields[i].Name.ToString() + ",";
                    }

                    if (fields.Length == 0)
                    {
                        s += kv.Value.GetType().Name + ",";
                    }

                    s += "\n";
                }

                // 数据
                s += kv.Key.ToString() + ",";

                for (int i = 0; i < fields.Length; i++)
                {
                    s += fields[i].GetValue(kv.Value).ToString() + ",";
                }

                if (fields.Length == 0)
                {
                    s += kv.Value.ToString() + ",";
                }

                if (c < dic.Count - 1)
                {
                    s += "\n";
                }
                c += 1;
            }
            return s;
        }

    }
}
