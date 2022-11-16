using Marten.Events;

namespace RideSharingMessages;

public enum DriverStatus
{
    Ready,
    Assigned,
    Unavailable
}

public record ShiftStarted(Guid DriverId, string Category, string PostalCode);

public record DriverReady(Location Location);
public record RideAccepted(Guid RideId, Location Location);
public record RideEnded(double Mileage, Location Location);

public class DriverShift
{
    public Guid Id { get; set; }
    
    // This will matter later!!!
    public int Version { get; set; }

    public DriverShift(IEvent<ShiftStarted> @event)
    {
        Day = @event.Timestamp.Date;
        DriverId = @event.Data.DriverId;
        Category = @event.Data.Category;
        PostalCode = @event.Data.PostalCode;
    }

    public DateTime Day { get; set; }

    public string PostalCode { get; set; }

    public string Category { get; set; }

    public Guid DriverId { get; set; }
    public DriverStatus Status { get; set; }
    
    public Location Location { get; set; }

    public void Apply(DriverReady ready)
    {
        Status = DriverStatus.Ready;
        Location = ready.Location;
    }

    public void Apply(RideAccepted accepted)
    {
        Status = DriverStatus.Assigned;
        RideId = accepted.RideId;
        Location = accepted.Location;
    }

    public void Apply(RideEnded ended)
    {
        Status = DriverStatus.Unavailable;
        Location = ended.Location;
        RideId = null;
    }

    public Guid? RideId { get; set; }
}