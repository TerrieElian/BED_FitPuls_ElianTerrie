# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0.102 AS build
WORKDIR /src

# Werkende system-protoc installeren: de arm64-protoc die Grpc.Tools zelf meelevert crasht (bekende bug)
RUN apt-get update && apt-get install -y --no-install-recommends protobuf-compiler && rm -rf /var/lib/apt/lists/*

COPY . .
RUN dotnet restore "FitPulse.csproj"

# Enkel op arm64 (bv. Apple Silicon) is de meegeleverde protoc-binary van Grpc.Tools kapot (bekende bug).
# Op amd64 (o.a. GitHub Actions-runners) werkt de standaard tooling gewoon, dus daar niets aanpassen.
RUN if [ "$(dpkg --print-architecture)" = "arm64" ]; then \
        export PROTOBUF_PROTOC=/usr/bin/protoc; \
        export GRPC_PROTOC_PLUGIN=$(find /root/.nuget/packages/grpc.tools -name grpc_csharp_plugin -path "*linux_arm64*" | head -1); \
    fi; \
    dotnet publish "FitPulse.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5180
EXPOSE 5181

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitPulse.dll"]
