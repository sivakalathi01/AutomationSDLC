using Virtusa.Agentic.Agents.IaCCicd.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5008");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IaCCicdAgentHandler>();

var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();