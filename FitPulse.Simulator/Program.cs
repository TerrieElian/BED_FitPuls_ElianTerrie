using Grpc.Net.Client;
using FitPulse.Grpc;

var grpcUrl = Environment.GetEnvironmentVariable("GrpcUrl") ?? "http://localhost:5181";
using var channel = GrpcChannel.ForAddress(grpcUrl);
var client = new Telemetry.TelemetryClient(channel);

var random = new Random();
const int deviceId = 2;
var sessionId = args.Length > 0 ? int.Parse(args[0]) : 1;
var apiKey = Environment.GetEnvironmentVariable("DEVICE_API_KEY") ?? "VUL_HIER_JE_EIGEN_DEVICE_API_KEY_IN";

Console.WriteLine($"Simulator gestart, verbind met {grpcUrl} (device {deviceId}, sessie {sessionId})");

var headers = new Grpc.Core.Metadata
{
    { "x-device-id", deviceId.ToString() },
    { "x-api-key", apiKey }
};

using var call = client.RecordReadings(headers);

string[] sensorTypes = { "Loopbandsensor", "Hartslagsensor", "Motor/Load-cell" };
string[] severities = { "Warning", "Critical" };

for (var i = 0; i < 10; i++)
{
    var reading = new TelemetryReadingRequest
    {
        SessionId = sessionId,
        DeviceId = deviceId,
        PowerWatts = Math.Round(random.NextDouble() * 200 + 50, 1),
        SpeedOrCadence = Math.Round(random.NextDouble() * 30 + 10, 1),
        DistanceOrReps = Math.Round(i * 0.1, 2),
        HeartRate = random.Next(110, 170),
        MotorTemp = Math.Round(random.NextDouble() * 10 + 30, 1)
    };

    await call.RequestStream.WriteAsync(reading);
    Console.WriteLine($"Verstuurd: {reading.PowerWatts}W, {reading.HeartRate} bpm");

    // Af en toe (~20% kans) ook een sensordiagnostiek-log sturen
    if (random.Next(100) < 20)
    {
        var diagnostic = new SensorDiagnosticRequest
        {
            DeviceId = deviceId,
            SensorType = sensorTypes[random.Next(sensorTypes.Length)],
            ErrorCode = $"ERR-{random.Next(100, 999)}",
            Severity = severities[random.Next(severities.Length)],
            RawSensorDataJson = "{\"raw\": \"" + Guid.NewGuid() + "\"}"
        };

        var diagnosticResponse = await client.ReportDiagnosticAsync(diagnostic, headers);
        Console.WriteLine($"Sensordiagnostiek verstuurd: {diagnostic.SensorType} - {diagnostic.ErrorCode} (geaccepteerd: {diagnosticResponse.Accepted})");
    }

    await Task.Delay(2000);
}

await call.RequestStream.CompleteAsync();
var summary = await call;

Console.WriteLine($"Server-antwoord: {summary.ProcessedCount} metingen verwerkt — {summary.StatusMessage}");