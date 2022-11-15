using Marten;
using RideSharingMessages;
using Wolverine;
using Wolverine.Attributes;

namespace RideSharingApi.Domain;

// This is *purposely* a long hand version
// public class RequestRideHandler
// {
//     private readonly IDocumentSession _session;
//     private readonly IMessagePublisher _publisher;
//
//     public RequestRideHandler(IDocumentSession session, IMessagePublisher publisher)
//     {
//         _session = session;
//         _publisher = publisher;
//     }
//
//     public async Task Handle(RequestRide command, CancellationToken cancellation)
//     {
//         // TODO -- we'll add some validation later
//         
//         var ride = new Ride
//         {
//             Starting = command.Starting,
//             Ending = command.Ending,
//             CustomerId = command.CustomerId,
//             Id = command.RideId
//         };
//         
//         _session.Store(ride);
//
//         // These two things need to succeed or fail together
//         await _publisher.EnqueueAsync(new RideRequested(ride.Id));
//         await _session.SaveChangesAsync(cancellation);
//     }
// }



public static class RequestRideHandler
{
    [Transactional]
    public static RideRequested Handle(RequestRide command, IDocumentSession session)
    {
        // TODO -- we'll add some validation later
        
        var ride = new Ride
        {
            Starting = command.Starting,
            Ending = command.Ending,
            CustomerId = command.CustomerId,
            Id = command.RideId
        };
        
        session.Store(ride);

        return new RideRequested(ride.Id);
    }
}