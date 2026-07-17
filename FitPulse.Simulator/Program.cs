using Grpc.Net.Client;
using FitPulse.Grpc;

var grpcUrl = Environment.GetEnvironmentVariable("GrpcUrl") ?? "http://localhost:5181";
using var channel = GrpcChannel.ForAddress(grpcUrl);
var client = new Telemetry.TelemetryClient(channel);

var random = new Random();
const int deviceId = 2;
const int sessionId = 1;
const string apiKey = "45e9fd7b-757c-419d-89d0-11e3aeda4678";

Console.WriteLine($"Simulator gestart, verbind met {grpcUrl} (device {deviceId}, sessie {sessionId})");

var headers = new Grpc.Core.Metadata
{
    { "x-device-id", deviceId.ToString() },
    { "x-api-key", apiKey }
};

using var call = client.RecordReadings(headers);

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

    await Task.Delay(2000);
}

await call.RequestStream.CompleteAsync();
var summary = await call;

Console.WriteLine($"Server-antwoord: {summary.ProcessedCount} metingen verwerkt — {summary.StatusMessage}");