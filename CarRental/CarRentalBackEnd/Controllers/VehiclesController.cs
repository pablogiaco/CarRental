using Microsoft.AspNetCore.Mvc;
using DTOs.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;


namespace CarRentalBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly CarRentalDbContext _context;

        public VehiclesController(CarRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<ActionResult<Vehicle>> AddVehicle(Vehicle vehicle)
        {
            if (vehicle.DailyPrice <= 0) return BadRequest("Daily price must be grater than 0");
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();
            
            if (_context.Rentals.Any(x => x.VehicleId == id && x.EndDate.Date >= DateTime.Now.Date && !x.IsCancelled))
                return BadRequest("Vehicle has a pending rental");

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
