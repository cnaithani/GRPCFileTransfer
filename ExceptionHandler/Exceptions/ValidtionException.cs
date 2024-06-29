using ExceptionHandler.Models;
using System.Text.Json.Serialization;

namespace ExceptionHandler.Exceptions
{
    [ExceptionSerializer(typeof(ValidationExceptionSerializer))]
    public class ValidtionException:Exception
    {
        public List<ValidationFaliurModel> ValidationFaliurs { get; set; }

        [JsonConstructor]
        public ValidtionException() { }

        public ValidtionException(params ValidationFaliurModel[] validationFaliurs) : base()
        {
            this.ValidationFaliurs = validationFaliurs.ToList();
        }
    }
}
