using Microsoft.AspNetCore.Mvc;
using DTOs.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using log4net;
using System.Text.Json;
using CarRentalBackEnd.Utils;


namespace CarRentalBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class VehiclesController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly ILog _log = LogManager.GetLogger(typeof(VehiclesController));

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
            _log.Debug($"Adding vehicle: {JsonSerializer.Serialize(vehicle)}");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();
            
            if (VehicleHasAPendingRental(id)) return BadRequest(Constants.VEHICLE_HAS_A_PENDING_RENTAL_ERROR_MESSAGE);
            
            _log.Debug($"Removing vehicle: {JsonSerializer.Serialize(vehicle)}");
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool VehicleHasAPendingRental(int id)
        {
            return _context.Rentals.Any(
                x => x.VehicleId == id 
                && x.EndDate.Date >= DateTime.UtcNow 
                && !x.IsCancelled);
        }
    }
}
