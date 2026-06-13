using Virtusa.Agentic.Agents.CodeGeneration.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5007");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<CodeGenerationAgentHandler>();

var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();