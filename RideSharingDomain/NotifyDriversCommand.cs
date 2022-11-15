namespace RideSharingMessages;

public record NotifyDriversCommand(Ride Ride, Guid[] DriverIdArray);