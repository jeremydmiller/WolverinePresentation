using Alba;
using Baseline.Dates;
using Microsoft.Extensions.Hosting;
using Oakton;
using RideSharingApi.Domain;
using RideSharingMessages;
using Shouldly;
using Wolverine.Tracking;
using Xunit;
using Xunit.Abstractions;

namespace RideSharingApi.Tests;

public class AppFixture : IAsyncLifetime
{
    public IAlbaHost Host { get; private set; }

    public async Task InitializeAsync()
    {
        OaktonEnvironment.AutoStartHost = true;
        Host = await AlbaHost.For<Program>(waf => { });
    }

    public Task DisposeAsync()
    {
        return Host.DisposeAsync().AsTask();
    }
}

public class RequestRideIntegrationTest : IClassFixture<AppFixture>
{
    private readonly AppFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RequestRideIntegrationTest(AppFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task request_a_ride_within_one_process()
    {
        var starting = new Location(30.266666, -97.733330);
        var ending = new Location(30.2668, -97.73355);

        var rideId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var command = new RequestRide(rideId, customerId, starting, ending);

        // Wolverine test helpers
        var session = await _fixture.Host
            .InvokeMessageAndWaitAsync(command);

        // Should have published a RideRequested event
        session.Sent.SingleMessage<RideRequested>()
            .RideId.ShouldBe(rideId);

        // Should have cascaded, and then spawned a NotifyDriversCommand message
        session.Sent.SingleMessage<NotifyDriversCommand>()
            .Ride.Id.ShouldBe(rideId);
    }

    [Fact]
    public async Task request_a_ride_across_processes()
    {
        using var notificationService = await NotificationService.Program
            .ConfigureHostBuilder(Array.Empty<string>())
            .StartAsync();
        
        var starting = new Location(30.266666, -97.733330);
        var ending = new Location(30.2668, -97.73355);

        var rideId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var command = new RequestRide(rideId, customerId, starting, ending);

        // Wolverine test helpers
        var session = await _fixture.Host
            .TrackActivity()
            .AlsoTrack(notificationService)
            .InvokeMessageAndWaitAsync(command);

        // Should have published a RideRequested event
        session.Sent.SingleMessage<RideRequested>()
            .RideId.ShouldBe(rideId);

        // Should have cascaded, and then spawned a NotifyDriversCommand message
        session.Received.SingleMessage<NotifyDriversCommand>()
            .Ride.Id.ShouldBe(rideId);
    }

    [Fact]
    public async Task call_the_web_service()
    {
        var starting = new Location(30.266666, -97.733330);
        var ending = new Location(30.2668, -97.73355);

        var rideId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var command = new RequestRide(rideId, customerId, starting, ending);

        // I'm using Alba to exercise the HTTP endpoint because this is easier
        // to demo in a presentation that me clicking around on a Swagger screen
        await _fixture.Host
            .Scenario(x => x.Post.Json(command).ToUrl("/ride/request"));
    }
    
    
}