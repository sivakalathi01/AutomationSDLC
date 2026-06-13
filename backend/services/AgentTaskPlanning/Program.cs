using Virtusa.Agentic.Agents.TaskPlanning.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5005");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<TaskPlanningAgentHandler>();

var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();