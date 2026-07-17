namespace FitPulse.Data;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IOptions<MongoSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<TelemetryReading> TelemetryReadings =>
        _database.GetCollection<TelemetryReading>("TelemetryReadings");

    public IMongoCollection<SensorDiagnosticLog> SensorDiagnosticLogs =>
        _database.GetCollection<SensorDiagnosticLog>("SensorDiagnosticLogs");
}
