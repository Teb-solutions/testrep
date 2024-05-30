using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Globalization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using EasyGas.Services.Profiles;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;
using EasyGas.Services.Profiles.Data;
using Microsoft.EntityFrameworkCore;

var configuration = GetConfiguration();

    Log.Logger = CreateSerilogLogger(configuration);

    try
    {
        Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
        var host = BuildWebHost(configuration, args);

    Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
        db.Database.Migrate();

        #region Seed data
        var tenant = db.Tenants.FirstOrDefault();
        if (tenant == null)
        {
            db.Tenants.Add(new EasyGas.Services.Profiles.Models.Tenant
            {
                Name = "TotalEnergies"
            });
            db.SaveChanges();
        }

        var branch = db.Branches.FirstOrDefault();
        if (branch == null)
        {
            db.Branches.Add(new EasyGas.Services.Profiles.Models.Branch
            {
                Name = "Branch 1",
                IsActive = true,
                CallCenterNumber = "0000000000",
                TenantId = db.Tenants.First().Id,
                Email = "jayadev.chandran@external.totalenergies.com",
                Mobile = "0000000000",
                Lat = 12,
                Lng = 77,
                Location = "",
            });
            db.SaveChanges();
        }

        var adminUser = db.Users.Where(p => p.Type == EasyGas.Shared.Enums.UserType.ADMIN).FirstOrDefault();
        if (adminUser == null)
        {
            db.Users.Add(new EasyGas.Services.Profiles.Models.User
            {
                TenantId = db.Tenants.First().Id,
                CognitoUsername = "AadLiftPP_jayadev.chandran@external.totalenergies.com",
                UserName = "AadLiftPP_jayadev.chandran@external.totalenergies.com",
                CreationType = EasyGas.Services.Profiles.Models.CreationType.USER,
                IsApproved = true,
                Type = EasyGas.Shared.Enums.UserType.ADMIN,
                IsDeleted = false,
                ApprovedAt = DateTime.Now,
                Profile = new EasyGas.Services.Profiles.Models.UserProfile
                {
                    AgreedTerms = true,
                    IsDeleted = false,
                    FirstName = "Tebs Admin",
                    CreatedAt = DateTime.Now,
                    Email = "jayadev.chandran@external.totalenergies.com"
                }
            });
            db.SaveChanges();
        }
        #endregion
    }

    /*
    host.MigrateDbContext<OrderingContext>((context, services) =>
    {
    var env = services.GetService<IWebHostEnvironment>();
    var settings = services.GetService<IOptions<OrderingSettings>>();
    var logger = services.GetService<ILogger<OrderingContextSeed>>();

    new OrderingContextSeed()
        .SeedAsync(context, env, settings, logger)
        .Wait();
    })
    .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });
    */

    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);

        host.Run();

        return 0;
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
        return 1;
    }
    finally
    {
        Log.CloseAndFlush();
    }

    IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(false)
            .ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(httpsOption =>
        {
            httpsOption.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("teb202425.pfx", "teb2025!!");
        });
        /*
        var ports = GetDefinedPorts(configuration);
        options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        });

        options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
        {
        listenOptions.Protocols = HttpProtocols.Http2;
        });
        */

    })
            .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
            .UseStartup<Startup>()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseSerilog()
            .Build();

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    var teamsWebhookUrl = configuration["Serilog:TeamsWebhookUrl"];

    /*
    Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
               = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
    aiOptions.EnableAdaptiveSampling = false;
    aiOptions.EnableRequestTrackingTelemetryModule = false;
    aiOptions.EnableDependencyTrackingTelemetryModule = false;
    */

    //var telemetryConfiguration = TelemetryConfiguration
    //  .CreateDefault();
    //telemetryConfiguration.InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];

    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        //.WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        //.WriteTo.MicrosoftTeams(teamsWebhookUrl, titleTemplate: "Error Logs", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
        //.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
        //.WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

IConfiguration GetConfiguration()
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    var config = builder.Build();

    /*
    if (config.GetValue<bool>("UseVault", false))
    {
        TokenCredential credential = new ClientSecretCredential(
        config["Vault:TenantId"],
        config["Vault:ClientId"],
        config["Vault:ClientSecret"]);
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }
    */
    return builder.Build();
}

    (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
    {
        var grpcPort = config.GetValue("GRPC_PORT", 5001);
        var port = config.GetValue("PORT", 80);
        return (port, grpcPort);
    }

    public partial class Program
    {
        public static string Namespace = typeof(Startup).Namespace;
        //public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
    public static string AppName = Namespace;
}




/* old
namespace EasyGas.Services.Profiles
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    // Requires using RazorPagesMovie.Models;
                    //SeedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    //var logger = services.GetRequiredService<ILogger<Program>>();
                    //logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            culture.DateTimeFormat.LongTimePattern = "";
            Thread.CurrentThread.CurrentCulture = culture;
            //Console.WriteLine(DateTime.Now);

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            //.UseKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>()
                .Build();
    }
}
*/
