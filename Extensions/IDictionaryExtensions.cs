using System;
using System.Collections.Generic;

namespace RosterApiLambda.Extensions
{
    public static class IDictionaryExtensions
    {
        public static bool GetBoolean(this IDictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out object value) ? Convert.ToBoolean(value.ToString()) : false;
        }

        public static int? GetInt(this IDictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out object value) ? (int?)Convert.ToInt32(value.ToString()) : null;
        }

        public static string GetString(this IDictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out object value) ? value.ToString() : null;
        }
    }
}
