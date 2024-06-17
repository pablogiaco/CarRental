using DataAccess;
using DTOs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using log4net;
using System.Text.Json;
using CarRentalBackEnd.Utils;


namespace CarRentalBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RentalsController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly ILog _log = LogManager.GetLogger(typeof(RentalsController));

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
            //We assume date times are in UTC
            if(!IsValidDate(rental)) return BadRequest(Constants.INVALID_DATE_RANGE_ERROR_MESSAGE);

            if (! await IsValidClient(rental)) return BadRequest(Constants.CLIENT_DOES_NOT_EXIST_ERROR_MESSAGE);

            if (! await IsValidVehicle(rental)) return BadRequest(Constants.VEHICLE_IS_NOT_AVAILABLE_ERROR_MESSAGE);
            
            var totalDays = (rental.EndDate - rental.StartDate).TotalDays;
            rental.TotalPrice = (decimal)totalDays * rental.Vehicle!.DailyPrice;
            rental.IsCancelled = false;

            _log.Debug($"Creating rental: {JsonSerializer.Serialize(rental)}");
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();
            return Ok(rental);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelRental(int id)
        {
            var rental = await _context.Rentals.Include(r => r.Client).Include(r => r.Vehicle).FirstOrDefaultAsync(x=> x.Id == id);
            if (rental == null) return NotFound();

            if (rental.IsCancelled) return BadRequest(Constants.RENTAL_IS_ALREADY_CANCELED_ERROR_MESSAGE);

            rental.IsCancelled = true;

            _log.Debug($"Canceling rental: {JsonSerializer.Serialize(rental)}");
            await _context.SaveChangesAsync();
            return Ok(rental);
        }

        private bool IsValidDate(Rental rental)
        {
            //We assume you can't rent a car for less than a day
            return rental.EndDate.Date > rental.StartDate.Date 
                && rental.StartDate.Date >= DateTime.UtcNow.Date;
        }

        private async Task<bool> IsValidClient(Rental rental)
        {
            //We assume the same client can rent as many cars has he wants
            var client = await _context.Clients.FindAsync(rental.ClientId);

            if (client == null) return false;

            rental.Client = client;
            return true;
        }

        private async Task<bool> IsValidVehicle(Rental rental)
        {
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
       
            //We asume you can't rent a car the same day its being returned from a previous rental.
            if (vehicle == null || _context.Rentals.Any(
                x => x.VehicleId == vehicle.Id 
                && !x.IsCancelled
                && x.StartDate.Date <= rental.EndDate.Date 
                && x.EndDate.Date >= rental.StartDate.Date))
            return false;

            rental.Vehicle = vehicle;
            return true;
        }
    }
}
