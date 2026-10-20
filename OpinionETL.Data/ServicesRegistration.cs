using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpinionETL.Core.Interfaces;
using OpinionETL.Data.Context;
using OpinionETL.Data.Importers;

namespace OpinionETL.Data
{
    public static class ServicesRegistration
    {
        public static void AddDataServices(this IServiceCollection services, IConfiguration config)
        {
            string connectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AppDbContext>((serviceProvider, opt) =>
            {
                opt.UseSqlServer(connectionString);
            },
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Scoped);

            services.AddScoped<IClientImporter, ClientImporter>();
            services.AddScoped<ICategoryImporter, CategoryImporter>();
        }
    }
}
