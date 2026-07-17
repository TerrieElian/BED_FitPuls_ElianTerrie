namespace FitPulse.Endpoints;

public static class MaintenanceLogEndpoints
{
    public static RouteGroupBuilder MapMaintenanceLogEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMaintenanceLogService service, IMapper mapper) =>
        {
            var logs = await service.GetAllMaintenanceLogsAsync();
            return Results.Ok(mapper.Map<List<MaintenanceLogDto>>(logs));
        });

        group.MapGet("/{id}", async (int id, IMaintenanceLogService service, IMapper mapper) =>
        {
            var log = await service.GetMaintenanceLogByIdAsync(id);
            return log is not null ? Results.Ok(mapper.Map<MaintenanceLogDto>(log)) : Results.NotFound();
        });

        group.MapPost("/", async (MaintenanceLog log, IMaintenanceLogService service) =>
        {
            try
            {
                await service.CreateMaintenanceLogAsync(log);
                return Results.Created($"/maintenancelogs/{log.Id}", log);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
            }
        });

        group.MapPut("/{id}", async (int id, MaintenanceLog log, IMaintenanceLogService service) =>
        {
            try
            {
                log.Id = id;
                await service.UpdateMaintenanceLogAsync(log);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
            }
        });

        group.MapDelete("/{id}", async (int id, IMaintenanceLogService service) =>
        {
            await service.DeleteMaintenanceLogAsync(id);
            return Results.NoContent();
        });

        return group;
    }
}