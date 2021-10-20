using System.Collections.Generic;

namespace IoTHubToADTAdapter
{
    internal static class DictionaryEx
    {
        public static bool TryGetTypedValue<TKey, TValue, TActual>(this IDictionary<TKey, TValue> data, TKey key, out TActual value) where TActual : TValue
        {
            if (data.TryGetValue(key, out TValue tmp))
            {
                value = (TActual)tmp;
                return true;
            }
            value = default;
            return false;
        }
    }
}
