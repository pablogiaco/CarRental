using CarRentalBackEnd.Controllers;
using CarRentalBackEnd.Utils;
using DataAccess;
using DTOs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackEndTests
{
    public class ClientsControllerTests
    {
        private ClientsController? _controller;
        private CarRentalDbContext? _context;
        private const string FIRST_NAME_1 = "Carlos";
        private const string LAST_NAME_1 = "Gonzales";
        private const string FIRST_NAME_2 = "Ivana";
        private const string LAST_NAME_2 = "Fuentes";


        [Test]
        public async Task GetClients_ReturnsAllClients()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
            .UseInMemoryDatabase(databaseName: nameof(GetClients_ReturnsAllClients))
                .Options;
            _context = new CarRentalDbContext(options);
            _controller = new ClientsController(_context);

            var client1 = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1};
            var client2 = new Client { FirstName = FIRST_NAME_2, LastName = LAST_NAME_2};
            await _controller.AddClient(client1);
            await _controller.AddClient(client2);            

            //Act
            var actionResult = await _controller.GetClients();
            var objectResult = actionResult.Result as ObjectResult;
            var clients = objectResult!.Value as IEnumerable<Client>;

            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(clients, !Is.Null);
            Assert.That(clients.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task AddClient_AddsClientToDatabase()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AddClient_AddsClientToDatabase))
                .Options;

            _context = new CarRentalDbContext(options);
            _controller = new ClientsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };

            //Act
            var actionResult = await _controller.AddClient(client);         
            var objectResult = actionResult.Result as ObjectResult;
            var clientResult = objectResult!.Value as Client;
            var clientInDb = _context.Clients.SingleOrDefault(x => x.Id == client.Id);

            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(clientResult!.FirstName, Is.EqualTo(FIRST_NAME_1));
            Assert.That(clientResult.LastName, Is.EqualTo(LAST_NAME_1));           
            Assert.That(clientInDb, !Is.Null);
            Assert.That(clientInDb.Id, Is.EqualTo(client.Id));
            Assert.That(clientInDb.FirstName, Is.EqualTo(FIRST_NAME_1));
            Assert.That(clientInDb.LastName, Is.EqualTo(LAST_NAME_1));          
        }

        [Test]
        public async Task RemoveClient_RemovesClientFromDatabase()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(RemoveClient_RemovesClientFromDatabase))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new ClientsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1};
            await _controller.AddClient(client);            

            //Act
            var result = await _controller.RemoveClient(client.Id) as NoContentResult;

            //Assert
            Assert.That(result, !Is.Null);
            Assert.That(_context.Clients.Count(x => x.Id == client.Id), Is.Zero);
        }

        [Test]
        public async Task RemoveClient_ReturnNotFound()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(RemoveClient_ReturnNotFound))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new ClientsController(_context);

            //Act
            var result = await _controller.RemoveClient(1) as NotFoundResult;

            //Assert
            Assert.That(result, !Is.Null);
        }

        [Test]
        public async Task RemoveClient_ReturnBadRequest()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(RemoveClient_ReturnBadRequest))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new ClientsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1};
            await _controller.AddClient(client);

            var date = DateTime.UtcNow;
            var rental = new Rental 
            { 
                ClientId = client.Id, 
                StartDate = date.AddDays(-1), 
                EndDate = date.AddDays(1) 
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            //Act
            var result = await _controller.RemoveClient(1) as BadRequestObjectResult;

            //Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Value, Is.EqualTo(Constants.CLIENT_HAS_A_PENDING_RENTAL_ERROR_MESSAGE));
        }

        [TearDown]
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
