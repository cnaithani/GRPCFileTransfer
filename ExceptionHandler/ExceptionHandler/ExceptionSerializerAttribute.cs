using Newtonsoft.Json;


namespace ExceptionHandler
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExceptionSerializerAttribute : Attribute
    {
        public Type SerializerType { get; }
        public ExceptionSerializerAttribute(Type serializerType)
        {
            SerializerType = serializerType;
        }
    }
    public interface IExceptionSerializer
    {
        void Serialize(Exception exception, Stream stream);
        Exception Deserialize(Stream stream);
    }
    public class CapturedExceptionMeta
    {
        [JsonProperty("ExceptionType")]
        public Type ExceptionType { get; }
        [JsonProperty("Serialized")]
        public byte[] Serialized { get; }
        [JsonConstructor]
        public CapturedExceptionMeta(Type exceptionType, byte[] serialized)
        {
            ExceptionType = exceptionType;
            Serialized = serialized;
        }
    }
}
