using Swashbuckle.AspNetCore.Annotations;

namespace DTOs.Models
{
    public class Vehicle
    {
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
    }
}
