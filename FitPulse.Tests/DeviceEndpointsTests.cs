public class DeviceEndpointsTests : IClassFixture<DeviceApiFactory>
{
    private readonly HttpClient _client;

    public DeviceEndpointsTests(DeviceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDevices_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/devices");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
