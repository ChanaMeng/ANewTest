using System;
using System.Text;
using Newtonsoft.Json;

namespace SDClub.Model
{
    public static class SerializeHelper
    {
        public static byte[] Serialize(object obj)
        {
            // 简单 JSON 序列化
            var json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }
        
        public static T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        public static object Deserialize(Type type, byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}
