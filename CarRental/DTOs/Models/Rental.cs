using Swashbuckle.AspNetCore.Annotations;

namespace DTOs.Models
{
    public class Rental
    {
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [SwaggerSchema(ReadOnly = true)]
        public decimal TotalPrice { get; set; }
        [SwaggerSchema(ReadOnly = true)]
        public bool IsCancelled { get; set; } = false;
    }
}
