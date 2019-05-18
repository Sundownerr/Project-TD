using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game.Utility.Serialization
{
    public static class SerializerDeserializerExtensions
    {
        public static byte[] Serializer(this object obj)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                IFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, obj);
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }

        public static T Deserializer<T>(this byte[] _byteArray)
        {
            T returnValue;
            using (var memoryStream = new MemoryStream(_byteArray))
            {
                IFormatter binaryFormatter = new BinaryFormatter();
                returnValue = (T)binaryFormatter.Deserialize(memoryStream);
            }
            return returnValue;
        }
    }
}