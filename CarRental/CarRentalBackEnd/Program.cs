using DataAccess;
using CarRentalBackEnd.Filters;
using Microsoft.EntityFrameworkCore;
using CarRentalBackEnd.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => options.Filters.Add<ExceptionHandlerFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLog4net();
builder.Services.AddSwaggerGen(options=> options.EnableAnnotations());
builder.Services.AddDbContext<CarRentalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CarRentalAPI v1"));
app.UpdateDatabase();
app.Run();
