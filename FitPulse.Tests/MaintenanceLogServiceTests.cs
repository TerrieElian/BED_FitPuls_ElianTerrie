namespace FitPulse.Tests;

public class MaintenanceLogServiceTests
{
    [Fact]
    public async Task CreateMaintenanceLogAsync_ValidLog_CallsRepository()
    {
        var repositoryMock = new Mock<IMaintenanceLogRepository>();
        var validatorMock = new Mock<IValidator<MaintenanceLog>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<MaintenanceLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var service = new MaintenanceLogService(repositoryMock.Object, validatorMock.Object);
        var log = new MaintenanceLog { DeviceId = 1, Date = DateTime.UtcNow, Description = "Test", TechnicianName = "Jan", Cost = 10 };

        await service.CreateMaintenanceLogAsync(log);

        repositoryMock.Verify(r => r.AddAsync(log), Times.Once);
    }

    [Fact]
    public async Task CreateMaintenanceLogAsync_InvalidLog_ThrowsAndDoesNotCallRepository()
    {
        var repositoryMock = new Mock<IMaintenanceLogRepository>();
        var validatorMock = new Mock<IValidator<MaintenanceLog>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<MaintenanceLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Date", "Date must be in the past") }));

        var service = new MaintenanceLogService(repositoryMock.Object, validatorMock.Object);
        var log = new MaintenanceLog { DeviceId = 1, Date = DateTime.UtcNow.AddDays(1), Description = "Test", TechnicianName = "Jan", Cost = 10 };

        Func<Task> act = () => service.CreateMaintenanceLogAsync(log);

        await act.Should().ThrowAsync<ValidationException>();
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<MaintenanceLog>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMaintenanceLogAsync_ValidLog_CallsRepository()
    {
        var repositoryMock = new Mock<IMaintenanceLogRepository>();
        var validatorMock = new Mock<IValidator<MaintenanceLog>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<MaintenanceLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var service = new MaintenanceLogService(repositoryMock.Object, validatorMock.Object);
        var log = new MaintenanceLog { Id = 1, DeviceId = 1, Date = DateTime.UtcNow, Description = "Bijgewerkt", TechnicianName = "Jan", Cost = 15 };

        await service.UpdateMaintenanceLogAsync(log);

        repositoryMock.Verify(r => r.UpdateAsync(log), Times.Once);
    }
}