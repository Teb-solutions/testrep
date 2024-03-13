using EasyGas.Services.Profiles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class BloggingContextFactory : IDesignTimeDbContextFactory<ProfilesDbContext>
    {
        public ProfilesDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfigurationRoot configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           //.AddJsonFile("appsettings.json")
           .AddJsonFile($"appsettings.Staging.json")
           .Build();

            
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            //var constr = Configuration["ConnectionStrings:DefaultConnection"];
            var optionsBuilder = new DbContextOptionsBuilder<ProfilesDbContext>();
            //optionsBuilder.UseSqlServer("Server =easygasservicesprofilesdbserver.database.windows.net;Database=EasyGasProfileDB; user id=ntn@easygasservicesprofilesdbserver; password=Tebs1234; MultipleActiveResultSets=true;");
            //optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=easygas-services-profiles; user id=ntn; password=tebs1234; MultipleActiveResultSets=true;");
            //optionsBuilder.UseSqlServer("Server=easygasdb1.westindia.cloudapp.azure.com,1433;Database=EasyGasProfileDB1; user id=toiri_admin; password=54EwU=q(4Nzu; MultipleActiveResultSets=true;");
            //optionsBuilder.UseSqlServer(connectionString, x => x.UseNetTopologySuite());
            optionsBuilder.UseNpgsql(connectionString, x => x.UseNetTopologySuite());

            return new ProfilesDbContext(optionsBuilder.Options);
        }
    }
}
