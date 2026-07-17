using System.Text.Json;
using System.Text.Json.Serialization;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FitPulseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
builder.Services.AddScoped<IMaintenanceLogService, MaintenanceLogService>();

builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();

builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();
builder.Services.AddScoped<ITrainingSessionService, TrainingSessionService>();

builder.Services.AddGrpc();

builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();

builder.Services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddTypeExtension<TrainingSessionExtensions>()
    .AddTypeExtension<DeviceExtensions>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DeviceProfile>();
    cfg.AddProfile<MaintenanceLogProfile>();
    cfg.AddProfile<MemberProfile>();
    cfg.AddProfile<TrainingSessionProfile>();
    cfg.AddProfile<SupportTicketProfile>();
    cfg.AddProfile<PaymentProfile>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageDevices", policy =>
        policy.RequireClaim("permissions", "manage:devices"));
    options.AddPolicy("ManageTickets", policy =>
        policy.RequireClaim("permissions", "manage:tickets"));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<DeviceValidator>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5180, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenAnyIP(5181, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FitPulseDbContext>();
    dbContext.Database.Migrate();
}

app.UseCors("Dashboard");

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/devices").RequireAuthorization("ManageDevices").MapDeviceEndpoints();

app.MapGroup("/maintenancelogs").RequireAuthorization("ManageDevices").MapMaintenanceLogEndpoints();

app.MapGroup("/members").RequireAuthorization().MapMemberEndpoints();

app.MapGroup("/trainingsessions").RequireAuthorization().MapTrainingSessionEndpoints();

app.MapGroup("/supporttickets").RequireAuthorization().MapSupportTicketEndpoints();

app.MapGraphQL("/graphql").RequireAuthorization();

app.UseWhen(
    context => context.Connection.LocalPort == 5181,
    appBuilder => appBuilder.UseMiddleware<DeviceApiKeyMiddleware>());


app.MapGrpcService<TelemetryIngestService>();

app.Run();

public partial class Program { }
