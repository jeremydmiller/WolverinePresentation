using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideSharingApi.Domain;
using RideSharingMessages;
using Wolverine.EntityFrameworkCore;

namespace RideSharingApi.EfCoreSamples;

public class RideDbContext : DbContext
{
    public DbSet<Ride> Rides { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ride>(map =>
        {
            map.ToTable("rides");
            map.HasKey(x => x.Id);

            var starting = map.OwnsOne(x => x.Starting);
            starting.Property(x => x.Latitude).HasColumnName("starting_latitude");
            starting.Property(x => x.Longitude).HasColumnName("starting_longitude");
            
            var ending = map.OwnsOne(x => x.Ending);
            ending.Property(x => x.Latitude).HasColumnName("ending_latitude");
            ending.Property(x => x.Longitude).HasColumnName("ending_longitude");
        });
    }
}

public class RequestRideController : ControllerBase
{
    [HttpPost("/ride/request")]
    public async Task Post(
        [FromBody] RequestRide command,
        [FromServices] IDbContextOutbox<RideDbContext> outbox)
    {
        var ride = new Ride
        {
            Starting = command.Starting,
            Ending = command.Ending,
            CustomerId = command.CustomerId,
            Id = command.RideId
        };

        outbox.DbContext.Rides.Add(ride);
        var message = new RideRequested(ride.Id);

        await outbox.PublishAsync(message);

        // Commit the unit of work and send outgoing
        // messages
        await outbox.SaveChangesAndFlushMessagesAsync();
    }
}