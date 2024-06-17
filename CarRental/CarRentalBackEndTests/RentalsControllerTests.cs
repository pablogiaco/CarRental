using CarRentalBackEnd.Controllers;
using CarRentalBackEnd.Utils;
using DataAccess;
using DTOs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackEndTests
{
    public class RentalsControllerTests
    {
        private RentalsController? _controller;
        private CarRentalDbContext? _context;
        private const string FIRST_NAME_1 = "Carlos";
        private const string LAST_NAME_1 = "Gonzales";
        private const string FIRST_NAME_2 = "Ivana";
        private const string LAST_NAME_2 = "Fuentes";
        private const string CAR_MODEL = "Civic";
        private const string CAR_BRAND = "Honda";
        private const decimal DAILY_PRICE = 70;


        [Test]
        public async Task GetRentals_ReturnsAllRentals()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
            .UseInMemoryDatabase(databaseName: nameof(GetRentals_ReturnsAllRentals))
                .Options;
            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            var client1 = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };
            var client2 = new Client { FirstName = FIRST_NAME_2, LastName = LAST_NAME_2 };
            await _context.Clients.AddRangeAsync(new List<Client> { client1, client2});

            var vehicle1 = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            var vehicle2 = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _context.Vehicles.AddRangeAsync(new List<Vehicle> {vehicle1, vehicle2 });
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow;
            var rental1 = new Rental 
            { 
                ClientId = client1.Id,                 
                VehicleId = vehicle1.Id,               
                StartDate = date, 
                EndDate = date.AddDays(1)
            };
            var rental2 = new Rental
            {
                ClientId = client2.Id,                
                VehicleId = vehicle2.Id,             
                StartDate = date,
                EndDate = date.AddDays(1)
            };

            await _controller.CreateRental(rental1);
            await _controller.CreateRental(rental2);           

            //Act
            var actionResult = await _controller.GetRentals();
            var objectResult = actionResult.Result as ObjectResult;
            var rentals = objectResult!.Value as IEnumerable<Rental>;

            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(rentals, !Is.Null);
            Assert.That(rentals.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task CreateRental_AddsRentalToDatabase()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(CreateRental_AddsRentalToDatabase))
                .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };
            await _context.Clients.AddAsync(client);
            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow;
            var rental = new Rental
            {
                ClientId = client.Id,              
                VehicleId = vehicle.Id,               
                StartDate = date,
                EndDate = date.AddDays(1)
            };

            //Act
            var result = await _controller.CreateRental(rental);            
            var objectResult = result.Result as ObjectResult;
            var RentalResult = objectResult!.Value as Rental;
            var RentalInDb = _context.Rentals.Include(r => r.Client).Include(r => r.Vehicle).SingleOrDefault(x => x.Id == client.Id);

            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(RentalResult!.IsCancelled, Is.EqualTo(false));
            Assert.That(RentalResult.Id, Is.EqualTo(rental.Id));
            Assert.That(RentalResult.VehicleId, Is.EqualTo(vehicle.Id));
            Assert.That(RentalResult.ClientId, Is.EqualTo(client.Id));
            Assert.That(RentalResult.StartDate, Is.EqualTo(rental.StartDate));
            Assert.That(RentalResult.EndDate, Is.EqualTo(rental.EndDate));
            Assert.That(RentalInDb, !Is.Null);            
            Assert.That(RentalInDb.IsCancelled, Is.EqualTo(false));
            Assert.That(RentalInDb.VehicleId, Is.EqualTo(vehicle.Id));
            Assert.That(RentalInDb.ClientId, Is.EqualTo(client.Id));
            Assert.That(RentalInDb.StartDate, Is.EqualTo(rental.StartDate));
            Assert.That(RentalInDb.EndDate, Is.EqualTo(rental.EndDate));
        }

        [Test]
        public async Task CreateRentalWithInvalidDates_ReturnsBadRequest()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(CreateRentalWithInvalidDates_ReturnsBadRequest))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };
            await _context.Clients.AddAsync(client);
            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow;
            var rental = new Rental
            {
                ClientId = client.Id,
                VehicleId = vehicle.Id,
                StartDate = date.AddDays(1),
                EndDate = date
            };

            //Act
            var result = await _controller.CreateRental(rental);
            var objectResult = result.Result as BadRequestObjectResult;
            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.Value, Is.EqualTo(Constants.INVALID_DATE_RANGE_ERROR_MESSAGE));         
        }

        [Test]
        public async Task CreateRentalWithInvalidClient_ReturnsBadRequest()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(CreateRentalWithInvalidClient_ReturnsBadRequest))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);
           
            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow;
            var rental = new Rental
            {
                ClientId = 1,             
                VehicleId = vehicle.Id,              
                StartDate = date,
                EndDate = date.AddDays(1)
            };

            //Act
            var result = await _controller.CreateRental(rental);
            var objectResult = result.Result as BadRequestObjectResult;
            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.Value, Is.EqualTo(Constants.CLIENT_DOES_NOT_EXIST_ERROR_MESSAGE));
        }

        [Test]
        public async Task CreateRentalWithInvalidVehicle_ReturnsBadRequest()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(CreateRentalWithInvalidVehicle_ReturnsBadRequest))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var date = DateTime.UtcNow;
            var rental = new Rental
            {
                ClientId = client.Id,          
                StartDate = date,
                EndDate = date.AddDays(1)
            };

            //Act
            var result = await _controller.CreateRental(rental);
            var objectResult = result.Result as BadRequestObjectResult;
            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.Value, Is.EqualTo(Constants.VEHICLE_IS_NOT_AVAILABLE_ERROR_MESSAGE));
        }

        [Test]
        public async Task CancelRental_CancelsRentalOnDatabase()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(CancelRental_CancelsRentalOnDatabase))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };
            await _context.Clients.AddAsync(client);
            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _context.Vehicles.AddAsync(vehicle);

            var date = DateTime.UtcNow;
            var rental = new Rental
            {
                ClientId = client.Id,               
                VehicleId = vehicle.Id,             
                StartDate = date,
                EndDate = date.AddDays(1)
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            //Act
            var result = await _controller.CancelRental(rental.Id) as OkObjectResult;
            var rentalResult = result!.Value as Rental;
            var rentalInDb = _context.Rentals.SingleOrDefault(x => x.Id == rental.Id);

            //Assert
            Assert.That(result, !Is.Null);
            Assert.That(rentalResult, !Is.Null);
            Assert.That(rentalResult.Id, Is.EqualTo(rental.Id));
            Assert.That(rentalResult.IsCancelled, Is.EqualTo(true));
            Assert.That(rentalResult.ClientId, Is.EqualTo(client.Id));
            Assert.That(rentalResult.VehicleId, Is.EqualTo(vehicle.Id));
            Assert.That(rentalResult.StartDate, Is.EqualTo(rental.StartDate));
            Assert.That(rentalResult.EndDate, Is.EqualTo(rental.EndDate));
            Assert.That(rentalInDb, !Is.Null);
            Assert.That(rentalInDb.IsCancelled, Is.EqualTo(true));
            Assert.That(rentalInDb.ClientId, Is.EqualTo(client.Id));
            Assert.That(rentalInDb.VehicleId, Is.EqualTo(vehicle.Id));
            Assert.That(rentalInDb.StartDate, Is.EqualTo(rental.StartDate));
            Assert.That(rentalInDb.EndDate, Is.EqualTo(rental.EndDate));
        }

        [Test]
        public async Task CancelRental_ReturnsNotFound()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(CancelRental_ReturnsNotFound))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            //Act
            var result = await _controller.CancelRental(1) as NotFoundResult;

            //Assert
            Assert.That(result, !Is.Null);
        }

        [Test]
        public async Task CancelRental_ReturnsBadRequest()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(CancelRental_ReturnsBadRequest))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new RentalsController(_context);

            var client = new Client { FirstName = FIRST_NAME_1, LastName = LAST_NAME_1 };
            await _context.Clients.AddAsync(client);
            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _context.Vehicles.AddAsync(vehicle);

            var date = DateTime.UtcNow;
            var rental = new Rental
            {
                ClientId = client.Id,               
                VehicleId = vehicle.Id,           
                StartDate = date,
                EndDate = date.AddDays(1)
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();
            await _controller.CancelRental(rental.Id);

            //Act
            var result = await _controller.CancelRental(rental.Id) as BadRequestObjectResult;

            //Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Value, Is.EqualTo(Constants.RENTAL_IS_ALREADY_CANCELED_ERROR_MESSAGE));
        }

        [TearDown]
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
