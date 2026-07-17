namespace FitPulse.Tests;

public class MatchingServiceTests : IClassFixture<DeviceApiFactory>
{
    private readonly DeviceApiFactory _factory;

    public MatchingServiceTests(DeviceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FindAvailableDeviceAsync_BeschikbaarToestel_GeeftHetTerug()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitPulseDbContext>();
        var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();

        var device = new Device { SerialNumber = "MATCH-001", LocationCode = "A1", Model = "Test", DeviceType = DeviceType.Strength, InstallationYear = 2024, IsActive = true };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var result = await matchingService.FindAvailableDeviceAsync(DeviceType.Strength);

        result.Should().NotBeNull();
        result!.Id.Should().Be(device.Id);
    }

    [Fact]
    public async Task FindAvailableDeviceAsync_ToestelZitAlInEenActieveSessie_WordtNietTeruggegeven()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitPulseDbContext>();
        var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();

        var device = new Device { SerialNumber = "MATCH-002", LocationCode = "A1", Model = "Test", DeviceType = DeviceType.Premium, InstallationYear = 2024, IsActive = true };
        dbContext.Devices.Add(device);

        var member = new Member { Auth0Subject = "matching-test-sub", Email = "matching@test.com" };
        dbContext.Users.Add(member);
        await dbContext.SaveChangesAsync();

        dbContext.TrainingSessions.Add(new TrainingSession
        {
            MemberId = member.Id,
            DeviceId = device.Id,
            Status = TrainingSessionStatus.InProgress,
            RequestedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var result = await matchingService.FindAvailableDeviceAsync(DeviceType.Premium);

        result.Should().BeNull();
    }
}