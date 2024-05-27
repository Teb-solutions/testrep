using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EasyGas.Services.Profiles.Data;
using Microsoft.EntityFrameworkCore;
using EasyGas.Services.Profiles.Queries;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyGas.Services.Profiles.Commands;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;
using EasyGas.Services.Core.Commands;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using EasyGas.Services.Profiles.BizLogic;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
//using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Wkhtmltopdf.NetCore;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.AspNetCore;
using Profiles.API.Infrastructure.Filters;
using Profiles.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Profiles.API.Infrastructure.AutofacModules;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Profiles.API.BizLogic;
using Profiles.API.Infrastructure.Services;
using EasyGas.Security;
using Azure.Storage.Blobs;
using Profiles.API.Infrastructure;
using Profiles.API.Services;
using MassTransit;
using Profiles.API.IntegrationEvents.Consumers;
using EasyGas.BuildingBlocks.IntegrationEventLogEF.Services;
using System.Data.Common;
using Microsoft.ApplicationInsights.Extensibility;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Dashboard;
using Profiles.API.IntegrationEvents;
using System.Net;
using Newtonsoft.Json;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EasyGas.Services.Profiles
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                //.AddGrpc(options =>
                //{
                //    options.EnableDetailedErrors = true;
                //})
                //.Services
                .AddApplicationInsights(Configuration)
                .AddCustomMvc()
                .AddHealthChecks(Configuration)
                .AddCustomDbContext(Configuration)
                .AddCustomSwagger(Configuration)
                .AddApplicationServices()
                .AddCustomIntegrations(Configuration)
                .AddCustomConfiguration(Configuration)
                //.AddEventBus(Configuration)
                .AddCustomAuthentication(Configuration);

            //TODO move to app services
            services.AddTransient<IProfileQueries, ProfileQueries>();
            services.AddTransient<IUserQueries, UserQueries>();
            services.AddTransient<ITenantQueries, TenantQueries>();
            //services.AddTransient<IOrderQueries, OrderQueries>();
            services.AddTransient<IVehicleQueries, VehicleQueries>();
            //services.AddTransient<IItemQueries, ItemQueries>();
            services.AddTransient<IReportQueries, ReportQueries>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<LocationService, LocationService>();
            //services.AddTransient<PlanService, PlanService>();
            //services.AddTransient<AssetService, AssetService>();
            //services.AddTransient<PlanMgr, PlanMgr>();
            //services.AddTransient<PlanSaver, PlanSaver>();
            services.AddTransient<NotificationMgr, NotificationMgr>();
            //services.AddTransient<OrderMgr, OrderMgr>();
            services.AddTransient<VehicleMgr, VehicleMgr>();
            //services.AddTransient<OrderRuleMgr, OrderRuleMgr>();
            services.AddTransient<GeoFenceMgr, GeoFenceMgr>();
            services.AddTransient<OtpMgr, OtpMgr>();
            //services.AddTransient<SettingsMgr, SettingsMgr>();
            services.AddTransient<ImportMgr, ImportMgr>();
            services.AddTransient<NotificationBroadcastMgr, NotificationBroadcastMgr>();
            //services.AddTransient<GoogleMapsApiService, GoogleMapsApiService>();
            services.AddTransient<IFeedbackQueries, FeedbackQueries>();
            //services.AddTransient<IDigitalSVService, DigitalSVService>();
            //services.AddTransient<IInvoiceService, InvoiceService>();
            services.AddTransient<IReportService, ReportService>();
            //services.AddTransient<PaymentMgr, PaymentMgr>();
            //services.AddTransient<RazorPayMgr, RazorPayMgr>();
            //services.AddTransient<EInvoiceMgr, EInvoiceMgr>();
            services.AddTransient<VoucherMgr, VoucherMgr>();
            services.AddTransient<WalletMgr, WalletMgr>();
            services.AddTransient<InvoiceMgr, InvoiceMgr>();
            //services.AddTransient<WalletApiService, WalletApiService>();
            services.AddTransient<NotificationSettingsJobMgr, NotificationSettingsJobMgr>();
            services.AddTransient<ProfileMgr, ProfileMgr>();
            services.AddTransient<IJWTUtils, JWTUtils>();
            services.AddTransient<IBusinessEntityQueries, BusinessEntityQueries>();
            services.AddTransient<SettingsMgr, SettingsMgr>();
            services.AddTransient<IImportService, ImportService>();
            services.AddTransient<PulzConnectMgr, PulzConnectMgr>();
            services.AddTransient<CrmMgr, CrmMgr>();
            //services.AddTransient<IProfilesIntegrationEventService, ProfilesIntegrationEventService>();

            /*
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage()
            );
            services.AddHangfireServer();
            */

            //configure autofac

            var container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterType<ProfilesCommandBus>().As<ICommandBus>();
            // Register CommandHandlers
            container.RegisterAssemblyTypes(typeof(Startup).GetTypeInfo().Assembly)
                 .AsClosedTypesOf(typeof(ICommandHandler<>))
                 .AsImplementedInterfaces()
                 .InstancePerLifetimeScope();

            container.RegisterModule(new MediatorModule());
            //container.RegisterModule(new ApplicationModule());
            //container.RegisterModule(new EventBusModule());

            //services.AddScoped<ExpressOrderCreatedIntegrationEventConsumer>();
            services.AddMassTransitHostedService();

            var build = container.Build();

            try
            {
                var bc = build.Resolve<IBusControl>();
                bc.Start();
            }
            catch(Exception ex)
            {

            }
            

            return new AutofacServiceProvider(build);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            //loggerFactory.AddApplicationInsights(app.ApplicationServices, Microsoft.Extensions.Logging.LogLevel.Information);


            app.UseSwagger(c =>
                {
                    c.RouteTemplate = "services/profiles/swagger/{documentName}/swagger.json";
                });

#if (DEBUG)
            app.UseDeveloperExceptionPage();
            //app.UseDatabaseErrorPage();
            //app.UseBrowserLink();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/services/profiles/swagger/v1/swagger.json", "EasyGas Profile Services API");
                options.RoutePrefix = "services/profiles/swagger/ui";
                options.DefaultModelsExpandDepth(1);
            });
#endif

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/services/profiles/swagger/v1/swagger.json", "Easygas Profiles API");
                    options.RoutePrefix = "services/profiles/swagger/ui";
                    options.DefaultModelsExpandDepth(1);
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }


            //app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");

            app.UseCors("CorsPolicy");
            app.UseRouting();
            
            ConfigureAuth(app);

            //app.UseStaticFiles();
            //app.UseDefaultFiles();

           // app.UseHangfireDashboard("/jobs", new DashboardOptions
           // {
            //    IsReadOnlyFunc = (DashboardContext context) => true
           // });
            //BackgroundJob.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
            /*
            RecurringJob.AddOrUpdate("cancelPreviousPickupOrders",
                () => serviceProvider.GetService<IJobMgr>().CancelYesterdaysPickupOrdersOfDriver(),
                "1 * * * *" // every hour first min
            ); 
            */

           // RecurringJob.AddOrUpdate("runNotificationCronJob",
           //     () => serviceProvider.GetService<IJobMgr>().RunNotificationCronJob(),
            //    "*/30 * * * *//" //every 30 min
           // );
            

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGrpcService<OrderingService>();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                /*endpoints.MapGet("/_proto/", async ctx =>
                {
                    ctx.Response.ContentType = "text/plain";
                    using var fs = new FileStream(Path.Combine(env.ContentRootPath, "Proto", "basket.proto"), FileMode.Open, FileAccess.Read);
                    using var sr = new StreamReader(fs);
                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();
                        if (line != "/* >>" || line != "<< */ //")
                        {
                            //await ctx.Response.WriteAsync(line);
                        }
                    //}
                //});*/

                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });

            ConfigureEventBus(app);
        }


        private void ConfigureEventBus(IApplicationBuilder app)
        {
            /*
            var eventBus = app.ApplicationServices.GetRequiredService<BuildingBlocks.EventBus.Abstractions.IEventBus>();

            eventBus.Subscribe<UserCheckoutAcceptedIntegrationEvent, IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent>>();
            eventBus.Subscribe<GracePeriodConfirmedIntegrationEvent, IIntegrationEventHandler<GracePeriodConfirmedIntegrationEvent>>();
            eventBus.Subscribe<OrderStockConfirmedIntegrationEvent, IIntegrationEventHandler<OrderStockConfirmedIntegrationEvent>>();
            eventBus.Subscribe<OrderStockRejectedIntegrationEvent, IIntegrationEventHandler<OrderStockRejectedIntegrationEvent>>();
            eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>>();
            eventBus.Subscribe<OrderPaymentSucceededIntegrationEvent, IIntegrationEventHandler<OrderPaymentSucceededIntegrationEvent>>();
         */
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }

    static class CustomExtensionsMethods
    {
         public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
         {
                
            Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
               = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
            aiOptions.EnableAdaptiveSampling = false;
            aiOptions.EnableRequestTrackingTelemetryModule = false;
            aiOptions.EnableDependencyTrackingTelemetryModule = false;
            //aiOptions.EnableQuickPulseMetricStream = false;
            services.AddApplicationInsightsTelemetry(aiOptions);

            //services.AddSingleton<ITelemetryInitializer, MyTelemetryInitializer>();
            //services.AddApplicationInsightsTelemetry(configuration);
            //services.AddApplicationInsightsKubernetesEnricher();

            return services;
         }

            public static IServiceCollection AddCustomMvc(this IServiceCollection services)
            {
                // Add framework services.
                services.AddControllers(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(HttpGlobalActionFilter));
                })
                    // Added for functional tests
                    .AddApplicationPart(typeof(ProfilesController).Assembly)
                    .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true)
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                        //.SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin()
                        //.AllowCredentials()
                        );
                });
            

            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .AddMvcOptions(options =>
            {
                //options.InputFormatters.Insert(0, new TextPlainInputFormatter());
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //register delegating handlers
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddSingleton<IJobMgr, JobMgr>();

            services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
                sp => (DbConnection c) => new IntegrationEventLogService(c));

            /*
            services.AddHttpClient<ICartService, CartApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ICatalogService, CatalogApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<IProfileService, ProfileApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<IPlanService, PlanApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ILocationService, LocationApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            */
            services.AddHttpClient<IWalletService, WalletApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<IOrderService, OrderApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ICartService, CartApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ICatalogService, CatalogApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ILocationService, LocationService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ICrmApiService, CrmApiService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
                var hcBuilder = services.AddHealthChecks();

                hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());
            /*
                hcBuilder
                    .AddSqlServer(
                        configuration["ConnectionStrings:DefaultConnection"],
                        name: "ProfilesDB-check",
                        tags: new string[] { "profilesdb" });

            string azureBlobConnection = configuration["ConnectionStrings:AzurBlobStorage"];
            if (!string.IsNullOrEmpty(azureBlobConnection))
            {
                hcBuilder
                    .AddAzureBlobStorage(
                        $"{azureBlobConnection}",
                        name: "profile-blobstorage-check",
                        tags: new string[] { "profileblobstorage" });
            }

            
            hcBuilder
                    .AddRabbitMQ(
                        $"{configuration["EventBusSettings:HostAddress"]}",
                        name: "profiles-rabbitmqbus-check",
                        tags: new string[] { "rabbitmqbus" });
            */

            //services.AddHealthChecksUI().AddInMemoryStorage();
            /*
                if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
                {
                    hcBuilder
                        .AddAzureServiceBusTopic(
                            configuration["EventBusConnection"],
                            topicName: "eshop_event_bus",
                            name: "ordering-servicebus-check",
                            tags: new string[] { "servicebus" });
                }
                else
                {
                    hcBuilder
                        .AddRabbitMQ(
                            $"amqp://{configuration["EventBusConnection"]}",
                            name: "ordering-rabbitmqbus-check",
                            tags: new string[] { "rabbitmqbus" });
                }
            */

            return services;
        }

            public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddDbContext<ProfilesDbContext>(options =>
                {
                    /*
                    options.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"],
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.UseNetTopologySuite();
                            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                            //sqlOptions.CommandTimeout(60);
                        });
                    */

                    options.UseNpgsql(configuration["ConnectionStrings:DefaultConnection"],
                    npgsqlOptionsAction: options =>
                    {
                        options.UseNetTopologySuite();
                        options.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                        options.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), null);
                    });
                },
                       ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
                   );

            /*
                services.AddDbContext<IntegrationEventLogContext>(options =>
                {
                    options.UseSqlServer(configuration["ConnectionString"],
                                         sqlServerOptionsAction: sqlOptions =>
                                         {
                                             sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                         });
                });
            */

                return services;
            }

            public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddSwaggerGen(options =>
                {
                    //options.DescribeAllEnumsAsStrings();
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "EasyGas - Profiles HTTP API",
                        Version = "v1",
                        Description = "The Profiles Service HTTP API"
                    });

                    OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                       Example: 'Bearer 12345abcdef'",
                    };

                    options.AddSecurityDefinition("Bearer", securityDefinition);
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,

                        },
                        new List<string>()
                      }
                    });


                    /*
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows()
                        {
                            Implicit = new OpenApiOAuthFlow()
                            {
                                AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                                TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                                Scopes = new Dictionary<string, string>()
                            {
                                { "orders", "Ordering API" }
                            }
                            }
                        }
                    });

                    options.OperationFilter<AuthorizeCheckOperationFilter>();
                    */

                });

                return services;
        }

        public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(_ => {
                return new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage"));
            });
            services.AddSingleton<IConfiguration>(configuration);
            services.AddWkhtmltopdf("wkhtmltopdf");

            /*
            services.AddTransient<IIdentityService, IdentityService>();
                services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
                    sp => (DbConnection c) => new IntegrationEventLogService(c));

                services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();
            */
            /*
                if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
                {
                    services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                    {
                        var serviceBusConnectionString = configuration["EventBusConnection"];
                        var serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);
                        var subscriptionClientName = configuration["SubscriptionClientName"];

                        return new DefaultServiceBusPersisterConnection(serviceBusConnection, subscriptionClientName);
                    });
                }
                else
                {
                    services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();


                        var factory = new ConnectionFactory()
                        {
                            HostName = configuration["EventBusConnection"],
                            DispatchConsumersAsync = true
                        };

                        if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                        {
                            factory.UserName = configuration["EventBusUserName"];
                        }

                        if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                        {
                            factory.Password = configuration["EventBusPassword"];
                        }

                        var retryCount = 5;
                        if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                        {
                            retryCount = int.Parse(configuration["EventBusRetryCount"]);
                        }

                        return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                    });
                }
            */
            return services;
        }

        public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Instance = context.HttpContext.Request.Path,
                            Status = StatusCodes.Status400BadRequest,
                            Detail = "Please refer to the errors property for additional details."
                        };

                        return new BadRequestObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json", "application/problem+xml" }
                        };
                    };
                });

                return services;
        }

        /*
            public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
            {
                if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
                {
                    services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                    {
                        var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                        var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                        var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                        var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                        return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                            eventBusSubcriptionsManager, iLifetimeScope);
                    });
                }
                else
                {
                    services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                    {
                        var subscriptionClientName = configuration["SubscriptionClientName"];
                        var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                        var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                        var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                        var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                        var retryCount = 5;
                        if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                        {
                            retryCount = int.Parse(configuration["EventBusRetryCount"]);
                        }

                        return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
                    });
                }

                services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

                return services;
            }
        */

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
                // prevent from mapping "sub" claim to nameidentifier.
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var tokenSecretKey = Encoding.ASCII.GetBytes(configuration["ApiSettings:JwtTokenPrivateKey"]);
            var tokenIssuer = configuration["ApiSettings:JwtTokenIssuer"];
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var tokenAud = configuration["Cognito:AppClientID"];
            var ValidIssuer = configuration["Cognito:Issuer"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Cognito";
                options.DefaultChallengeScheme = "Cognito";
            })
            .AddJwtBearer("Cognito", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                    //{
                    // get JsonWebKeySet from AWS
                    //var json = new WebClient().DownloadString(parameters.ValidIssuer + "/.well-known/jwks.json");
                    // serialize the result
                    //var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;
                    // cast the result to be the type expected by IssuerSigningKeyResolver
                    //return (IEnumerable<SecurityKey>)keys;
                    //},
                    SignatureValidator = (token, _) => new JsonWebToken(token),
                    ValidateIssuerSigningKey = false,  // Disable signature validation
                    RequireSignedTokens = false,
                    ValidateIssuer = false,
                    //ValidIssuer = configuration["Cognito:Issuer"],
                    ValidateAudience = false,
                    //ValidAudience = configuration["Cognito:AppClientID"],
                    ValidateLifetime = false,
                };
            })
            .AddJwtBearer("APIGateway", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Cognito:Issuer"],
                    ValidateAudience = false,
                    //ValidAudience = configuration["Cognito:Audience"],
                    ValidateLifetime = true,
                    //IssuerSigningKey = new SymmetricSecurityKey(
                        //Convert.FromBase64String(Configuration["Jwt:APIGateway:SecretKey"]))
                };
            });


            /*
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.RequireHttpsMetadata = false;
                jwtOptions.SaveToken = true;

                jwtOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(tokenSecretKey),
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = tokenIssuer,
                    ClockSkew = TimeSpan.FromSeconds(10)
                };

            });
            */

            /*
            var identityUrl = configuration.GetValue<string>("IdentityUrl");
            
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;

                }).AddJwtBearer(options =>
                {
                    options.Authority = identityUrl;
                    options.RequireHttpsMetadata = false;
                    options.Audience = "orders";
                });
            */
            return services;
        }
    }
}
