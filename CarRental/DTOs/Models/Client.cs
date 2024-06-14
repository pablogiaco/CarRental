using Swashbuckle.AspNetCore.Annotations;

namespace DTOs.Models
{
    public class Client
    {
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
