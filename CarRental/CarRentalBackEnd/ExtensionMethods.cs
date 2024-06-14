using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackEnd
{
    public static class ExtensionMethods
    {
        public static void CreateDatabaseIfNotExists(this WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
                context.Database.Migrate();
            }
        }
    }
}
