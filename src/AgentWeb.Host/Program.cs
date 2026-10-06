using AgentWeb.Host.Agents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISiteAgent, ContentAgent>();
builder.Services.AddSingleton<ISiteAgent, EstimateAgent>();
builder.Services.AddSingleton<AgentRegistry>();
builder.Services.AddSingleton<NavigationAgent>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

app.MapGet("/api/agents", (AgentRegistry registry) => registry.GetManifests());
app.MapGet("/api/navigation", (NavigationAgent navigation) => navigation.Build());

app.MapGet("/api/experience/{capability}", async (string capability, AgentRegistry registry) =>
{
    var response = await registry.ExecuteAsync(capability);

    return response is null
        ? Results.NotFound(new { error = $"No agent provides capability '{capability}'." })
        : Results.Ok(response);
});

app.Run();
