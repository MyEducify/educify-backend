using System.Text.Json;

namespace Utils.ObjectUtil
{
    public class Utility
    {
        public static string SerializeData<T>(T data)
        {
            return JsonSerializer.Serialize(data);
        }
        public static T? DeserializeData<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
