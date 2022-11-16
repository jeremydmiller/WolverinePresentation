using Marten;
using Marten.Events.Projections;
using Oakton;
using Oakton.Resources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RideSharingApi.Domain;
using RideSharingMessages;
using Wolverine;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Adds in some expanded command line diagnostics, not
// hugely important
builder.Host.ApplyOaktonExtensions();

// Application services
builder.Services.AddScoped<IDriverLocator, DriverLocator>();

// That's it for now
builder.Host.UseWolverine(opts =>
{
    // Sigh, bug found while doing this demo. Necessary for the test harness.
    opts.ApplicationAssembly = typeof(Program).Assembly;

    opts.UseRabbitMq()
        .AutoProvision()
        .AutoPurgeOnStartup()  // Strictly for testing
        .UseConventionalRouting()
        .ConfigureSenders(x => x.UseDurableOutbox());

    // Explicit routing
    opts.PublishMessage<RideAccepted>().ToRabbitQueue("ride-accepted");
});

// Using Marten for persistence
builder.Services.AddMarten(opts =>
    {
        opts.Connection(builder.Configuration.GetConnectionString("marten"));
        opts.DatabaseSchemaName = "ride_sharing";

        opts.Projections
            .SelfAggregate<DriverShift>(ProjectionLifecycle.Inline);
    })

    // This adds Marten middleware support and uses Postgresql for the outbox
    .IntegrateWithWolverine()
    
    // Automatically publish events in Marten that have an active
    // subscription
    .EventForwardingToWolverine();


// This directs the app to provision any known resources
// like Wolverine's inbox/outbox storage schema objects
// on application startup
builder.Services.AddResourceSetupOnStartup();


builder.Services.AddOpenTelemetryTracing(x =>
{
    x.SetResourceBuilder(ResourceBuilder
            .CreateDefault()
            .AddService("RideSharingApi")) // <-- sets service name

        .AddJaegerExporter()
        .AddAspNetCoreInstrumentation()

        // This is absolutely necessary to collect the Wolverine
        // open telemetry tracing information in your application
        .AddSource("Wolverine");
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Just delegating to Wolverine here as a "mediator" tool
app.MapPost("/ride/request", (RequestRide command, ICommandBus bus) => bus.InvokeAsync(command));

// Expanded command line options
await app.RunOaktonCommands(args);