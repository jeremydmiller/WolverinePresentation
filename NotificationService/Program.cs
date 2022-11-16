using Marten;
using Oakton;
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
            });
    }
}