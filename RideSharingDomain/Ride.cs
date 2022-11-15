namespace RideSharingMessages;

public record Location(double Latitude, double Longitude);

public class Ride
{
    public Guid Id { get; set; }
    public Location Starting { get; set; }
    public Location Ending { get; set; }
    public Guid CustomerId { get; set; }

    public IReadOnlyList<ActiveDriver> Filter(IReadOnlyList<ActiveDriver> candidates)
    {
        // Apply some logic to choose candidate drivers and vehicles based
        // on who knows what criteria that the customer chose
        return candidates;
    }
}

public class ActiveDriver
{
    public Location Location { get; set; }
    public Driver Driver { get; set; }
    public Vehicle Vehicle { get; set; }
}

public class Vehicle
{
    public string Category { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
}