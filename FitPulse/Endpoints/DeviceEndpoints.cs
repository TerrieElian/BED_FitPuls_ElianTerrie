namespace FitPulse.Endpoints;

public static class DeviceEndpoints
{
    public static RouteGroupBuilder MapDeviceEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IDeviceService service, IMapper mapper) =>
        {
            var devices = await service.GetAllDevicesAsync();
            return Results.Ok(mapper.Map<List<DeviceDto>>(devices));
        });

        group.MapPost("/", async (Device device, IDeviceService service) =>
        {
            try
            {
                await service.CreateDeviceAsync(device);
                return Results.Created($"/devices/{device.Id}", device);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
            }
        });

        group.MapGet("/{id}", async (int id, IDeviceService service, IMapper mapper) =>
        {
            var device = await service.GetDeviceByIdAsync(id);
            return device is not null ? Results.Ok(mapper.Map<DeviceDto>(device)) : Results.NotFound();
        });

        group.MapPut("/{id}", async (int id, Device device, IDeviceService service) =>
        {
            try
            {
                device.Id = id;
                await service.UpdateDeviceAsync(device);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
            }
        });

        group.MapDelete("/{id}", async (int id, IDeviceService service) =>
        {
            await service.DeleteDeviceAsync(id);
            return Results.NoContent();
        });

        return group;
    }
}