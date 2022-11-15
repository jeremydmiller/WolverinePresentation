using RideSharingMessages;
using Wolverine.Attributes;

namespace RideSharingApi.Domain;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}


public record RequestRide(Guid RideId, Guid CustomerId, Location Starting, Location Ending);

public record RideRequested(Guid RideId);

