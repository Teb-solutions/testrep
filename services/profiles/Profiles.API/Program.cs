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

var configuration = GetConfiguration();

    Log.Logger = CreateSerilogLogger(configuration);

    try
    {
        Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
        var host = BuildWebHost(configuration, args);

    //Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);

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
            httpsOption.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("tebpfx2023.pfx", "teb123");
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
