namespace FitPulse.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Device> GetDevices(FitPulseDbContext context) => context.Devices;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<TrainingSession> GetTrainingSessions(FitPulseDbContext context) => context.TrainingSessions;
}