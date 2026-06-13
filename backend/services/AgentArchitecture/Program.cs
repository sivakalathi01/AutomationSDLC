using Virtusa.Agentic.Agents.Architecture.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5004");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<ArchitectureAgentHandler>();

var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();