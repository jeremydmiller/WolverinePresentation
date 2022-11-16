using Marten;
using RideSharingMessages;
using Wolverine.Marten;

namespace RideSharingApi.Domain;

public record AcceptRide(Guid DriverShiftId, Guid RideId, int Version);

public record DriverInbound();

public class AcceptRideHandler
{
    // The "Decider" pattern
    [MartenCommandWorkflow]
    public static IEnumerable<object> Handle(AcceptRide command, DriverShift aggregate)
    {
        // If unavailable, then no, you can't accept the ride
        if (aggregate.Status == DriverStatus.Unavailable)
        {
            yield break;
        }

        yield return new RideAccepted(command.RideId, aggregate.Location);

        if (aggregate.Status == DriverStatus.Ready)
        {
            yield return new DriverInbound();
        }
    }
}