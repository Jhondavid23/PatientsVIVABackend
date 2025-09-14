using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientsVIVABackend.BLL.PatientsService;
using PatientsVIVABackend.BLL.PatientsService.Contract;
using PatientsVIVABackend.Model;
using PatientsVIVABackend.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.IOC
{
    public static class Dependency
    {
        public static void InyectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Get conection string from environment variable
            var connectionString = Environment.GetEnvironmentVariable("VIVAPATIENTS_CONNECTION_STRING");

            // Inject DbContext with SQL Server provider
            services.AddDbContext<VivapatientsContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // Inyect repositories
            services.AddScoped(typeof(DAL.Repositories.IGenericRepository<>), typeof(DAL.Repositories.GenericRepository<>));
            // Inyect AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Inyect services
            services.AddScoped<IPatientService, PatientService>();


        }
    }
}
