using System;
using Newtonsoft.Json;

namespace Utils
{
    public static class JsonUtil
    {
        public static T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Stringify<T>(T entity)
        {
            return JsonConvert.SerializeObject(entity);
        }
    }
}
