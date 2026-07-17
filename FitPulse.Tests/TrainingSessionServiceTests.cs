namespace FitPulse.Tests;

public class TrainingSessionServiceTests
{
    private static TrainingSession CreateSession(int id, TrainingSessionStatus status) => new()
    {
        Id = id,
        MemberId = 1,
        DeviceId = 2,
        Status = status,
        RequestedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task RequestSessionAsync_DeviceAvailable_CreatesSessionWithRequestedStatus()
    {
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        var matchingServiceMock = new Mock<IMatchingService>();
        matchingServiceMock
            .Setup(m => m.FindAvailableDeviceAsync(DeviceType.Cardio))
            .ReturnsAsync(new Device { Id = 2, DeviceType = DeviceType.Cardio });

        var service = new TrainingSessionService(sessionRepositoryMock.Object, matchingServiceMock.Object);

        var session = await service.RequestSessionAsync(memberId: 1, DeviceType.Cardio);

        session.Status.Should().Be(TrainingSessionStatus.Requested);
        session.DeviceId.Should().Be(2);
        sessionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TrainingSession>()), Times.Once);
    }

    [Fact]
    public async Task RequestSessionAsync_GeenToestelBeschikbaar_ThrowsInvalidOperationException()
    {
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        var matchingServiceMock = new Mock<IMatchingService>();
        matchingServiceMock
            .Setup(m => m.FindAvailableDeviceAsync(It.IsAny<DeviceType>()))
            .ReturnsAsync((Device?)null);

        var service = new TrainingSessionService(sessionRepositoryMock.Object, matchingServiceMock.Object);

        Func<Task> act = () => service.RequestSessionAsync(memberId: 1, DeviceType.Premium);

        await act.Should().ThrowAsync<InvalidOperationException>();
        sessionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TrainingSession>()), Times.Never);
    }

    [Fact]
    public async Task StartSessionAsync_VanuitRequested_WordtInProgress()
    {
        var session = CreateSession(1, TrainingSessionStatus.Requested);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        var result = await service.StartSessionAsync(1);

        result.Status.Should().Be(TrainingSessionStatus.InProgress);
        result.StartedAt.Should().NotBeNull();
        sessionRepositoryMock.Verify(r => r.UpdateAsync(session), Times.Once);
    }

    [Fact]
    public async Task StartSessionAsync_VanuitCompleted_GooitInvalidOperationException()
    {
        var session = CreateSession(1, TrainingSessionStatus.Completed);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        Func<Task> act = () => service.StartSessionAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StartSessionAsync_SessieBestaatNiet_GooitKeyNotFoundException()
    {
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TrainingSession?)null);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        Func<Task> act = () => service.StartSessionAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CompleteSessionAsync_VanuitInProgress_WordtCompleted()
    {
        var session = CreateSession(1, TrainingSessionStatus.InProgress);
        session.StartedAt = DateTime.UtcNow.AddMinutes(-15);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        var result = await service.CompleteSessionAsync(1, caloriesBurned: 200);

        result.Status.Should().Be(TrainingSessionStatus.Completed);
        result.DurationMinutes.Should().BeInRange(15, 16); // afronding naar boven, kleine tijdsmarge tijdens het testen
        result.CaloriesBurned.Should().Be(200);
    }

    [Fact]
    public async Task CompleteSessionAsync_ZeerKorteSessie_WordtNaarBovenAfgerondNaarMinimum1Minuut()
    {
        var session = CreateSession(1, TrainingSessionStatus.InProgress);
        session.StartedAt = DateTime.UtcNow.AddSeconds(-5);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        var result = await service.CompleteSessionAsync(1, caloriesBurned: 10);

        result.DurationMinutes.Should().Be(1);
    }

    [Fact]
    public async Task CompleteSessionAsync_VanuitRequested_GooitInvalidOperationException()
    {
        var session = CreateSession(1, TrainingSessionStatus.Requested);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        Func<Task> act = () => service.CompleteSessionAsync(1, 200);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelSessionAsync_VanuitRequested_WordtCancelled()
    {
        var session = CreateSession(1, TrainingSessionStatus.Requested);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        var result = await service.CancelSessionAsync(1);

        result.Status.Should().Be(TrainingSessionStatus.Cancelled);
    }

    [Fact]
    public async Task CancelSessionAsync_VanuitCompleted_GooitInvalidOperationException()
    {
        var session = CreateSession(1, TrainingSessionStatus.Completed);
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var service = new TrainingSessionService(sessionRepositoryMock.Object, Mock.Of<IMatchingService>());

        Func<Task> act = () => service.CancelSessionAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}