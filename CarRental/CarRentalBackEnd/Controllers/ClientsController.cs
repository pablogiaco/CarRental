using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess;
using DTOs.Models;

namespace CarRentalBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly CarRentalDbContext _context;

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
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();           
            
            if (_context.Rentals.Any(x => x.ClientId == id && x.EndDate.Date >= DateTime.Now.Date && !x.IsCancelled))
                return BadRequest("Client has a pending rental");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
