using Marten;
using Oakton;
using Oakton.Resources;
using RideSharingApi.Domain;
using Wolverine;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

builder.Services.AddScoped<IDriverLocator, DriverLocator>();

// That's it for now
builder.Host.UseWolverine(opts =>
{
    // TODO -- Jeremy needs to get rid of this
    opts.ApplicationAssembly = typeof(Program).Assembly;
});

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("marten"));
    opts.DatabaseSchemaName = "ride_sharing";
})
    
    // This adds Marten middleware support and uses Postgresql for the outbox
    .IntegrateWithWolverine();


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