using Marten;
using Marten.Events.Projections;
using Oakton;
using Oakton.Resources;
using RideSharingApi.Domain;
using RideSharingMessages;
using Wolverine;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

builder.Services.AddScoped<IDriverLocator, DriverLocator>();

// That's it for now
builder.Host.UseWolverine(opts =>
{
    // TODO -- Sigh, bug, necessary for testing. Jeremy needs to get rid of this
    opts.ApplicationAssembly = typeof(Program).Assembly;

    opts.UseRabbitMq()
        .AutoProvision()
        .AutoPurgeOnStartup()  // Strictly for testing
        .UseConventionalRouting()
        .ConfigureSenders(x => x.UseDurableOutbox());

    // Explicit routing
    opts.PublishMessage<RideAccepted>().ToRabbitQueue("ride-accepted");
});

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


builder.Services.AddResourceSetupOnStartup();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/ride/request", (RequestRide command, ICommandBus bus) => bus.InvokeAsync(command));

// Expanded command line options
await app.RunOaktonCommands(args);