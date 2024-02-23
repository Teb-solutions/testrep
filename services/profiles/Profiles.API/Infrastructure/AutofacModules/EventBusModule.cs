using Autofac;
using EasyGas.BuildingBlocks.EventBus.Abstractions;
using EasyGas.BuildingBlocks.EventBus.EventBusRabbitMQ;
using MassTransit;
using Profiles.API.IntegrationEvents.Consumers;
using System.Reflection;

namespace Profiles.API.Infrastructure.AutofacModules
{
    public class EventBusModule
        : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventBusRabbitMQ>()
                .As<IEventBus>()
                .InstancePerLifetimeScope();

            builder.AddMassTransit(x =>
            {
                // add all consumers in the specified assembly
                x.AddConsumers(Assembly.GetExecutingAssembly());

                // add consumers by type
                //x.AddConsumers(typeof(ConsumerOne), typeof(ConsumerTwo));

                // add the bus to the container
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("amqp://tebsadmin:tebsadmin112233@localhost:5672");

                    cfg.ReceiveEndpoint("profiles-customer-express-order-created-queue", ec =>
                    {
                        // Configure a single consumer
                        ec.ConfigureConsumer<CustomerExpressOrderCreatedIntegrationEventConsumer>(context);
                        // configure all consumers
                        //ec.ConfigureConsumers(context);

                        // configure consumer by type
                        //ec.ConfigureConsumer(typeof(ConsumerOne));
                    });

                    cfg.ReceiveEndpoint("profiles-customer-express-order-confirmed-queue", ec =>
                    {
                        ec.ConfigureConsumer<CustomerExpressOrderConfirmedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-customer-order-dispatched-queue", ec =>
                    {
                        ec.ConfigureConsumer<CustomerOrderDispatchedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-customer-delivery-order-confirmed-queue", ec =>
                    {
                        ec.ConfigureConsumer<CustomerDeliveryOrderConfirmedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-customer-pickup-order-confirmed-queue", ec =>
                    {
                        ec.ConfigureConsumer<CustomerPickupOrderConfirmedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-customer-order-delivered-queue", ec =>
                    {
                        ec.ConfigureConsumer<CustomerOrderDeliveredIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-customer-order-cancelled-queue", ec =>
                    {
                        ec.ConfigureConsumer<CustomerOrderCancelledIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-route-plan-changed-queue", ec =>
                    {
                        ec.ConfigureConsumer<RoutePlanChangedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-driver-pickup-order-confirmed-queue", ec =>
                    {
                        ec.ConfigureConsumer<DriverPickupOrderConfirmedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-user-rating-changed-queue", ec =>
                    {
                        ec.ConfigureConsumer<AverageRatingChangedIntegrationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-send-bulk-notification-queue", ec =>
                    {
                        ec.ConfigureConsumer<SendBulkPushNotificationEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("profiles-test-queue", ec =>
                    {
                        ec.ConfigureConsumer<TestIntegrationEventConsumer>(context);
                    });

                    // or, configure the endpoints by convention
                    //cfg.ConfigureEndpoints(context);
                });
                
            });

            builder.RegisterType<CustomerExpressOrderCreatedIntegrationEventConsumer>()
               .As<CustomerExpressOrderCreatedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<CustomerDeliveryOrderConfirmedIntegrationEventConsumer>()
               .As<CustomerDeliveryOrderConfirmedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<CustomerExpressOrderConfirmedIntegrationEventConsumer>()
               .As<CustomerExpressOrderConfirmedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<CustomerPickupOrderConfirmedIntegrationEventConsumer>()
               .As<CustomerPickupOrderConfirmedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<CustomerOrderDeliveredIntegrationEventConsumer>()
               .As<CustomerOrderDeliveredIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<RoutePlanChangedIntegrationEventConsumer>()
               .As<RoutePlanChangedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<CustomerOrderDispatchedIntegrationEventConsumer>()
               .As<CustomerOrderDispatchedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<DriverPickupOrderConfirmedIntegrationEventConsumer>()
               .As<DriverPickupOrderConfirmedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<CustomerOrderCancelledIntegrationEventConsumer>()
               .As<CustomerOrderCancelledIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<AverageRatingChangedIntegrationEventConsumer>()
               .As<AverageRatingChangedIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            builder.RegisterType<SendBulkPushNotificationEventConsumer>()
               .As<SendBulkPushNotificationEventConsumer>()
               .InstancePerLifetimeScope();

            builder.RegisterType<TestIntegrationEventConsumer>()
               .As<TestIntegrationEventConsumer>()
               .InstancePerLifetimeScope();
            //services.AddScoped<ExpressOrderCreatedIntegrationEventConsumer>();
            /*
            var container = builder.Build();
            var bc = container.Resolve<IBusControl>();
            bc.Start();
            */

            /*
            builder.RegisterGeneric(typeof(AutofacConsumerFactory<>))
    .WithParameter(new NamedParameter("name", "message"))
    .As(typeof(IConsumerFactory<>));
            */

        }
    }
}
