
public class DeviceServiceTests
{
    [Fact]
    public async Task CreateDeviceAsync_ValidDevice_CallsRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IDeviceRepository>();
        var validatorMock = new Mock<IValidator<Device>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // geen errors = geldig

        var service = new DeviceService(repositoryMock.Object, validatorMock.Object);
        var device = new Device { SerialNumber = "DEV-001", LocationCode = "A1", Model = "X", InstallationYear = 2024 };

        // Act
        await service.CreateDeviceAsync(device);

        // Assert
        repositoryMock.Verify(r => r.AddAsync(device), Times.Once);
    }

    [Fact]
    public async Task CreateDeviceAsync_InvalidDevice_ThrowsAndDoesNotCallRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IDeviceRepository>();
        var validatorMock = new Mock<IValidator<Device>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
            new ValidationFailure("SerialNumber", "SerialNumber is required")
            }));

        var service = new DeviceService(repositoryMock.Object, validatorMock.Object);
        var device = new Device { SerialNumber = "", LocationCode = "A1", Model = "X", InstallationYear = 2024 };

        // Act
        Func<Task> act = () => service.CreateDeviceAsync(device);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Device>()), Times.Never);
    }
}