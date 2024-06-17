using DataAccess;
using log4net.Config;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackEnd.Utils
{
    public static class ExtensionMethods
    {
        public static void UpdateDatabase(this WebApplication app)
        {
            using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
                context.Database.Migrate();
            }
        }

        public static void AddLog4net(this IServiceCollection services)
        {
            var logPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}log{Path.DirectorySeparatorChar}";
            GlobalContext.Properties["logPath"] = logPath;
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            services.AddSingleton(LogManager.GetLogger(typeof(Program)));
        }
    }
}
