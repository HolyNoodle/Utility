using System.IO;
using System.Runtime.Serialization.Json;

namespace HolyNoodle.Utility
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(T element)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(element.GetType());
                serializer.WriteObject(memoryStream, element);

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(memoryStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static T Deserialize<T>(string element)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var sr = new StreamWriter(memoryStream))
                {
                    sr.WriteLine(element);
                    sr.Flush();

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var serializer = new DataContractJsonSerializer(typeof(T));
                    return (T)serializer.ReadObject(memoryStream);
                }
            }
        }
    }
}
