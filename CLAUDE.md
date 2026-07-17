# CLAUDE.md — FitPulse Backend

Instructiebestand voor AI-coding agents (Claude Code) die aan dit project werken.
Dit bestand legt de gekozen architectuur en conventies vast, zodat de agent zich hier consistent aan houdt in plaats van elke sessie iets anders te verzinnen.

## Belangrijkste gedragsregel

De student moet elke regel code op het examen zelf kunnen uitleggen zonder AI. Genereer daarom **niet** in één keer een volledig project of een volledige feature end-to-end.

- Werk per laag/feature, niet het hele project ineens.
- Leg bij elk niet-triviaal stuk code kort uit **wat** het doet en **waarom**, zodat de student het in eigen woorden kan navertellen.
- Vraag bevestiging voor je een groot nieuw bestand of een volledige feature aanmaakt — laat de student meelezen en waar mogelijk zelf typen.
- Kleine, behapbare stappen boven grote "rap-rap" oplossingen.

## Project overzicht

- Taal: C# 14, target framework .NET 10 (`net10.0`)
- API-stijl: ASP.NET Core Minimal APIs (**geen** MVC controllers)
- Databases: PostgreSQL (Entity Framework Core) voor transactionele data, MongoDB (officiële driver) voor telemetrie
- Auth: API Keys voor toestel→cloud (device-ingest), OAuth2 via Auth0 voor leden & admins (Authorization Code Flow met PKCE, zoals in labo "08 Security Part 2"). Login/registratie van leden gebeurt bij Auth0 zelf (hosted login page) — **geen** eigen `/register`/`/login`-endpoint met wachtwoord-hashing in deze API. De API valideert enkel het binnenkomende JWT (`AddJwtBearer`) en gebruikt policies op basis van Auth0-scopes/permissions-claims (RBAC), niet op basis van een eigen rollen-kolom. Overweeg voor device-ingest ook de OAuth2 **Client Credential Flow** (machine-to-machine, ook behandeld in dat labo) als alternatief/aanvulling op de statische API-key.
- Extra protocollen naast REST: gRPC voor telemetrie-ingest, GraphQL (HotChocolate) voor dashboard/rapportage

## Projectstructuur

Eén plat Web API-project, zoals in de labo's — **geen** aparte Domain/Application/Infrastructure-projecten:

```
Configuration/   settings-classes (Mongo, Auth0, ApiKey, ...)
Data/            FitPulseDbContext (EF/Postgres) + MongoContext (Mongo driver wrapper)
DTO/             request/response DTOs
Endpoints/       1 static class per resource, extension-methode op RouteGroupBuilder
GraphQL/         Query/Mutation types (HotChocolate)
Grpc/            .proto files + service-implementaties (telemetrie-ingest)
Middleware/      ApiKeyMiddleware, GlobalExceptionHandler
Migrations/      EF Core migraties
Models/          EF entities
Repositories/    interface + implementatie per aggregate
Services/        business logica (o.a. PricingService)
Validators/      FluentValidation validators
Program.cs
```

## Public vs Internal API

Geen 2 aparte projecten — route groups + `.RequireAuthorization(policy)` in `Program.cs`. Dit is het lichtste patroon dat toch een letterlijke, aantoonbare scheiding tussen publieke en interne endpoints geeft, zonder dubbele infrastructuur:

- Publieke groepen (leden/mobile app): sessies aanvragen/starten, eigen profiel, betalingen → policy voor leden
- Interne/admin groepen (toestellenpark, onderhoud, tickets beheren) → aparte policy, gebaseerd op een Auth0-scope/permission-claim (bv. `manage:devices`), niet op een eigen rollen-kolom

Elk bestand in `Endpoints/` is een static class met een `Group<Resource>(this RouteGroupBuilder)` extensie-methode. Geen business logica in `Program.cs` zelf — enkel service-registratie en route-mapping.

## Coding conventies

- Repository & Service patroon overal. Interface en implementatie mogen samen in 1 bestand (bv. `IDeviceRepository` + `DeviceRepository` beide in `DeviceRepository.cs`) — geen aparte interface-bestanden nodig.
- FluentValidation voor alle input-DTO's.
- AutoMapper voor het mappen van entities naar response-DTO's (Profiles in een `Profiles/`-map) — komt terug in praktisch elke labo-oefening, dus verwacht bij dit vak.
- Serilog voor logging (console + file sink).
- Connection strings/secrets via environment variables of user-secrets — **niet** plaintext in `appsettings.json` committen.
- API-key vergelijking in `ApiKeyMiddleware` gebeurt via `CryptographicOperations.FixedTimeEquals` (timing-safe), niet via een gewone `==`/`.Equals()` string-vergelijking. Elk toestel/gebruiker heeft best zijn eigen key in de databank, geen 1 hardcoded globale key.
- `Program.cs` eindigt met `public partial class Program { }` zodra er integratietests met `WebApplicationFactory<Program>` bijkomen — anders vindt het testproject de class niet.
- gRPC naast REST in dezelfde app vereist 2 aparte Kestrel-poorten (HTTP/1 voor REST, apart HTTP/2 voor gRPC) via `builder.WebHost.ConfigureKestrel(...)`.
- GraphQL (HotChocolate) registratie: `AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>().AddFiltering().AddSorting().AddProjections().AddErrorFilter<ValidationErrorFilter>()` — de `ValidationErrorFilter` zet FluentValidation-exceptions om naar nette GraphQL-errors.
- **Global usings**: alle `using`-statements die in meerdere bestanden nodig zijn (bv. `Microsoft.EntityFrameworkCore`, `FitPulse.Models`, `MongoDB.Driver`, ...) horen thuis in `Usings.cs` als `global using`, niet los bovenaan elk bestand herhaald. Als je een nieuw bestand aanmaakt en een namespace nodig hebt die al globaal staat, laat de lokale `using` dan gewoon weg. Enkel een namespace die je maar in 1 bestand gebruikt, mag lokaal blijven staan.

## Pricing engine — exacte volgorde (kritiek, telt zwaar mee)

`PricingService` moet deze stappen in exact deze volgorde uitvoeren:

1. Basisprijs = sessietarief + (minuten × tarief) + (kcal × tarief)
2. Multiplier toestelType toepassen
3. Piekuur-opslag (17:00–21:00) op de prijs na stap 2
4. Loyalty-korting, geplafonneerd op 20% van de huidige prijs
5. Kortingscode valideren, dan toepassen op het bedrag ná loyalty-korting
6. BTW toevoegen (21%, expliciet gedocumenteerde aanname)
7. Minimumprijs afdwingen
8. Nooit negatief (expliciete check, ook al dekt stap 7 dit al af)
9. Afronden op 2 decimalen — pas als allerlaatste stap

Elke stap moet een eigen unit test hebben (zie checklist in het stappenplan-document).

## Testing

- Unit tests voor services, vooral `PricingService` en de matching-service (dichtstbijzijnd beschikbaar toestel).
- Integration tests met Testcontainers (echte Postgres + Mongo containers), niet in-memory DB.
