using ExceptionHandler.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExceptionHandler.Exceptions
{
    public class ValidationExceptionSerializer : IExceptionSerializer
    {
        public Exception Deserialize(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            string serialized = reader.ReadToEnd();
            var exception = JsonConvert.DeserializeObject<ValidtionException>(serialized);
            return exception;
        }

        public void Serialize(Exception exception, Stream stream)
        {
            Exceptions.ValidtionException validationException = (Exceptions.ValidtionException)exception;
            string serialized = JsonConvert.SerializeObject(validationException);
            StreamWriter writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            writer.Write(serialized);
            stream.Flush();
        }
    }
}
