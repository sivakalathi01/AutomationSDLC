using Virtusa.Agentic.Agents.QualityGate.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5009");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<QualityGateAgentHandler>();

var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();