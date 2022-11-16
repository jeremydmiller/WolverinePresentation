using Marten;
using RideSharingMessages;

namespace RideSharingApi.Domain;

public interface IDriverLocator
{
    Task<IReadOnlyList<ActiveDriver>> FindActiveDriversAsync(Location location);
}

public class DriverLocator : IDriverLocator
{
    public Task<IReadOnlyList<ActiveDriver>> FindActiveDriversAsync(Location location)
    {
        var list = new List<ActiveDriver>();
        return Task.FromResult<IReadOnlyList<ActiveDriver>>(list);
    }
}

public static class RideRequestedHandler
{
    public static async Task<NotifyDriversCommand> Handle(
        RideRequested @event,
        IQuerySession session,
        IDriverLocator locator)
    {
        var ride = await session.LoadAsync<Ride>(@event.RideId);

        var candidates = await locator.FindActiveDriversAsync(ride.Starting);

        var driverIds = ride
            .Filter(candidates)
            .Select(x => x.Driver.Id)
            .ToArray();

        return new NotifyDriversCommand(ride, driverIds);
    }
}