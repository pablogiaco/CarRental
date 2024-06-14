using CarRentalBackEnd.Controllers;
using DataAccess;
using DTOs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackEndTests
{
    public class VehiclesControllerTests
    {
        private VehiclesController? _controller;
        private CarRentalDbContext? _context;
        private const string CAR_MODEL = "Civic";
        private const string CAR_BRAND = "Honda";
        private const decimal DAILY_PRICE = 70;

        [Test]
        public async Task GetVehicles_ReturnsAllVehicles()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(GetVehicles_ReturnsAllVehicles))
                .Options;
            _context = new CarRentalDbContext(options);
            _controller = new VehiclesController(_context);

            var vehicle1 = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            var vehicle2 = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            await _controller.AddVehicle(vehicle1);
            await _controller.AddVehicle(vehicle2);

            //Act
            var actionResult = await _controller.GetVehicles();
            var objectResult = actionResult.Result as ObjectResult;
            var vehicles = objectResult!.Value as IEnumerable<Vehicle>;

            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(vehicles, !Is.Null);
            Assert.That(vehicles.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task AddVehicle_AddsVehicleToDatabase()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AddVehicle_AddsVehicleToDatabase))
                .Options;

            _context = new CarRentalDbContext(options);
            _controller = new VehiclesController(_context);

            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };

            //Act
            var result = await _controller.AddVehicle(vehicle);
            var objectResult = result.Result as ObjectResult;
            var vehicleResult = objectResult!.Value as Vehicle;
            var vehicleInDb = _context.Vehicles.SingleOrDefault(x => x.Id == vehicle.Id);

            //Assert
            Assert.That(objectResult, !Is.Null);
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(vehicleResult!.Model, Is.EqualTo(CAR_MODEL));
            Assert.That(vehicleResult.Brand, Is.EqualTo(CAR_BRAND));
            Assert.That(vehicleResult.DailyPrice, Is.EqualTo(DAILY_PRICE));
            Assert.That(vehicleInDb, !Is.Null);
            Assert.That(vehicleInDb.Id, Is.EqualTo(vehicle.Id));
            Assert.That(vehicleInDb.Model, Is.EqualTo(CAR_MODEL));
            Assert.That(vehicleInDb.Brand, Is.EqualTo(CAR_BRAND));
            Assert.That(vehicleInDb.DailyPrice, Is.EqualTo(DAILY_PRICE));
        }

        [Test]
        public async Task RemoveVehicle_RemovesVehicleFromDatabase()
        {

            //Arrange
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(RemoveVehicle_RemovesVehicleFromDatabase))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new VehiclesController(_context);

            var vehicle = new Vehicle { Model = CAR_MODEL, Brand = CAR_BRAND, DailyPrice = DAILY_PRICE };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            //Act
            var result = await _controller.RemoveVehicle(vehicle.Id) as NoContentResult;

            //Assert
            Assert.That(result, !Is.Null);
            Assert.That(_context.Vehicles.Count(x => x.Id == vehicle.Id), Is.Zero);
        }

        [Test]
        public async Task RemoveVehicle_ReturnNotFound()
        {
            var options = new DbContextOptionsBuilder<CarRentalDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(RemoveVehicle_ReturnNotFound))
               .Options;

            _context = new CarRentalDbContext(options);
            _controller = new VehiclesController(_context);

            //Act
            var result = await _controller.RemoveVehicle(1) as NotFoundResult;

            //Assert
            Assert.That(result, !Is.Null);
        }

        [TearDown]
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
