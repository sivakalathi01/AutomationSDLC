using Virtusa.Agentic.OrchestratorApi.Services;
using Microsoft.ApplicationInsights;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000");

// Configuration
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential(),
        new AzureKeyVaultConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromHours(1)
        });
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.Configure<AgentServiceOptions>(builder.Configuration.GetSection("Services"));
builder.Services.AddSingleton<IAgentServiceRegistry, AgentServiceRegistry>();
builder.Services.AddHttpClient<IAgentHttpClient, AgentHttpClient>();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<TelemetryClient>();

// Service Bus
var serviceBusConnectionString = builder.Configuration.GetConnectionString("ServiceBus");
if (!string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    builder.Services.AddSingleton(new Azure.Messaging.ServiceBus.ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddScoped<IServiceBusPublisher, ServiceBusPublisher>();
}
else
{
    builder.Services.AddScoped<IServiceBusPublisher, NoopServiceBusPublisher>();
}
builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();

// Cosmos DB
var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
if (!string.IsNullOrWhiteSpace(cosmosConnectionString))
{
    var cosmosClient = new Microsoft.Azure.Cosmos.CosmosClient(cosmosConnectionString);
    builder.Services.AddSingleton(cosmosClient);
    builder.Services.AddScoped<IEventStore, EventStore>();
}
else
{
    builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
}

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("ServiceBus", () =>
    {
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
    })
    .AddCheck("CosmosDb", () =>
    {
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policyBuilder =>
    {
        policyBuilder.WithOrigins(
            builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Optional JWT auth for enterprise environments
var configuredEnableAuthentication = builder.Configuration["Security:EnableAuthentication"];
var enableAuthentication = string.IsNullOrWhiteSpace(configuredEnableAuthentication)
    ? !builder.Environment.IsDevelopment()
    : builder.Configuration.GetValue<bool>("Security:EnableAuthentication");
var authority = builder.Configuration["Security:Jwt:Authority"];
var audience = builder.Configuration["Security:Jwt:Audience"];

if (!builder.Environment.IsDevelopment())
{
    var missingSettings = new List<string>();

    if (string.IsNullOrWhiteSpace(keyVaultUrl))
        missingSettings.Add("KeyVault:Url");

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("ServiceBus")))
        missingSettings.Add("ConnectionStrings:ServiceBus");

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("CosmosDb")))
        missingSettings.Add("ConnectionStrings:CosmosDb");

    if (string.IsNullOrWhiteSpace(builder.Configuration["AzureOpenAI:Endpoint"]))
        missingSettings.Add("AzureOpenAI:Endpoint");

    if (string.IsNullOrWhiteSpace(builder.Configuration["AzureOpenAI:ApiKey"]))
        missingSettings.Add("AzureOpenAI:ApiKey");

    if (string.IsNullOrWhiteSpace(builder.Configuration["AzureOpenAI:DeploymentName"]))
        missingSettings.Add("AzureOpenAI:DeploymentName");

    if (string.IsNullOrWhiteSpace(builder.Configuration["Services:Requirements"]))
        missingSettings.Add("Services:Requirements");

    if (string.IsNullOrWhiteSpace(builder.Configuration["Services:Specification"]))
        missingSettings.Add("Services:Specification");

    if (string.IsNullOrWhiteSpace(builder.Configuration["Services:Story"]))
        missingSettings.Add("Services:Story");

    if (enableAuthentication && string.IsNullOrWhiteSpace(authority))
        missingSettings.Add("Security:Jwt:Authority");

    if (enableAuthentication && string.IsNullOrWhiteSpace(audience))
        missingSettings.Add("Security:Jwt:Audience");

    if (missingSettings.Count > 0)
    {
        throw new InvalidOperationException(
            $"Missing required configuration values for environment '{builder.Environment.EnvironmentName}': {string.Join(", ", missingSettings)}");
    }
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EngineeringLeadOrAdmin", policy =>
        policy.RequireRole("EngineeringLead", "PlatformAdmin"));
    options.AddPolicy("SecurityOrAdmin", policy =>
        policy.RequireRole("SecurityReviewer", "PlatformAdmin"));
});

if (enableAuthentication && !string.IsNullOrWhiteSpace(authority) && !string.IsNullOrWhiteSpace(audience))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = builder.Configuration.GetValue("Security:RequireHttpsMetadata", false);
        });
}
else
{
    builder.Services
        .AddAuthentication(DevelopmentBypassAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevelopmentBypassAuthHandler>(
            DevelopmentBypassAuthHandler.SchemeName,
            _ => { });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactFrontend");
if (enableAuthentication)
{
    app.UseAuthentication();
}
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
