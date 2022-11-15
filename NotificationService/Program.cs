using Marten;
using Oakton;
using Wolverine;
using Wolverine.RabbitMQ;

return await Host.CreateDefaultBuilder(args)
    .UseWolverine((context, opts) =>
    {
        opts.UseRabbitMq()
            .AutoProvision()
            .UseConventionalRouting();

        opts.Services.AddMarten(opts =>
        {
            var connectionString = context.Configuration.GetConnectionString("marten");
            opts.Connection(connectionString);
            opts.DatabaseSchemaName = "ride_sharing";
        });


    })
    .RunOaktonCommands(args);