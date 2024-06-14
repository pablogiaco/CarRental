using DataAccess;
using DTOs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CarRentalBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly CarRentalDbContext _context;

        public RentalsController(CarRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rental>>> GetRentals()
        {
            var rentals = await _context.Rentals.Include(r => r.Client).Include(r => r.Vehicle).ToListAsync();
            return Ok(rentals);
        }

        [HttpPost]
        public async Task<ActionResult<Rental>> CreateRental(Rental rental)
        {
            if(!IsValidDate(rental)) return BadRequest("Incorrect dates");

            if (! await IsValidClient(rental)) return BadRequest("Client does not exists");

            if (! await IsValidVehicle(rental)) return BadRequest("Vehicle is not available");            

            var totalDays = (rental.EndDate - rental.StartDate).TotalDays;
            rental.TotalPrice = (decimal)totalDays * rental.Vehicle!.DailyPrice;
            rental.IsCancelled = false;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return Ok(rental);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            if (rental.IsCancelled) return BadRequest("Rental is already cancelled");

            rental.IsCancelled = true;

            await _context.SaveChangesAsync();
            return Ok(rental);
        }

        private bool IsValidDate(Rental rental)
        {
            return rental.EndDate.Date > rental.StartDate.Date && rental.StartDate.Date >= DateTime.Now.Date;
        }

        private async Task<bool> IsValidClient(Rental rental)
        {
            var client = await _context.Clients.FindAsync(rental.ClientId);

            if (client == null) return false;

            rental.Client = client;
            return true;
        }

        private async Task<bool> IsValidVehicle(Rental rental)
        {
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);

            //Checks if vehicle exists and if its available
            if (vehicle == null || _context.Rentals.Any(
                x => x.VehicleId == vehicle.Id && !rental.IsCancelled
                && x.StartDate.Date <= rental.EndDate.Date && x.EndDate.Date >= rental.StartDate.Date))
            return false;

            rental.Vehicle = vehicle;
            return true;
        }
    }
}
