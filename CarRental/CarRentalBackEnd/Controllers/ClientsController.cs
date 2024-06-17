using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess;
using DTOs.Models;
using log4net;
using System.Text.Json;
using CarRentalBackEnd.Utils;

namespace CarRentalBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClientsController : ControllerBase
    {
        private readonly CarRentalDbContext _context;
        private readonly ILog _log = LogManager.GetLogger(typeof(ClientsController));

        public ClientsController(CarRentalDbContext context)
        {
            _context = context;            
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var clients = await _context.Clients.ToListAsync(); 
            return Ok(clients);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> AddClient(Client client)
        {
            _log.Debug($"Adding client: {JsonSerializer.Serialize(client)}");
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();           
            
            if (ClientHasAPendingRental(id)) return BadRequest(Constants.CLIENT_HAS_A_PENDING_RENTAL_ERROR_MESSAGE);

            _log.Debug($"Removing client: {JsonSerializer.Serialize(client)}");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ClientHasAPendingRental(int id)
        {
            return _context.Rentals.Any(
                 x => x.ClientId == id
                 && x.EndDate.Date >= DateTime.UtcNow.Date
                 && !x.IsCancelled);
        }
    }

}
