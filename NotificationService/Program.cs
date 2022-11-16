using Marten;
using Oakton;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Wolverine;
using Wolverine.RabbitMQ;

namespace NotificationService;

public static class Program
{
    public static Task<int> Main(string[] args)
    {
        return ConfigureHostBuilder(args).RunOaktonCommands(args);
    }

    public static IHostBuilder ConfigureHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseWolverine((context, opts) =>
            {
                opts.ApplicationAssembly = typeof(Program).Assembly;
                
                opts.UseRabbitMq()
                    .AutoProvision()
                    .UseConventionalRouting();

                opts.Services.AddMarten(opts =>
                {
                    var connectionString = context.Configuration.GetConnectionString("marten");
                    opts.Connection(connectionString);
                    opts.DatabaseSchemaName = "ride_sharing";
                });
                
                opts.Services.AddOpenTelemetryTracing(x =>
                {
                    x.SetResourceBuilder(ResourceBuilder
                            .CreateDefault()
                            .AddService("NotificationService")) // <-- sets service name

                        .AddJaegerExporter()
                        .AddAspNetCoreInstrumentation()

                        // This is absolutely necessary to collect the Wolverine
                        // open telemetry tracing information in your application
                        .AddSource("Wolverine");
                });
            });
    }
}