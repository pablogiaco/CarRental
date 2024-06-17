using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DTOs.Models
{
    [SwaggerSchema(ReadOnly = true)]
    public class Vehicle
    {
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Invalid daily price value" )]
        public decimal DailyPrice { get; set; }
    }
}
