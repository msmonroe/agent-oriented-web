using AgentWeb.Contracts;
using AgentWeb.Host.Agents;

var builder = WebApplication.CreateBuilder(args);

var pluginDirectory = Path.Combine(builder.Environment.ContentRootPath, "plugins");
var discoveredAgents = PluginAgentLoader.Load(pluginDirectory);

builder.Services.AddSingleton<ISiteAgent, ContentAgent>();
foreach (var agent in discoveredAgents)
    builder.Services.AddSingleton(typeof(ISiteAgent), agent);

builder.Services.AddSingleton<AgentRegistry>();
builder.Services.AddSingleton<ExperienceResolver>();
builder.Services.AddSingleton<NavigationAgent>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

app.MapGet("/api/agents", (AgentRegistry registry) => registry.GetManifests());
app.MapGet("/api/capabilities", (AgentRegistry registry) => registry.GetCapabilityGraph());
app.MapGet("/api/viability", (ExperienceResolver resolver) => resolver.EvaluateAll());
app.MapGet("/api/navigation", (NavigationAgent navigation) => navigation.Build());

app.MapGet("/api/experience/{capability}", async (
    string capability,
    AgentRegistry registry,
    ExperienceResolver resolver) =>
{
    var viability = resolver.Evaluate(capability);
    if (!viability.Viable)
        return Results.Conflict(new { error = viability.Reason, capability });

    var response = await registry.ExecuteAsync(capability);
    return response is null
        ? Results.NotFound(new { error = $"No agent provides capability '{capability}'." })
        : Results.Ok(response);
});

app.MapPost("/api/action/{capability}", async (
    string capability,
    Dictionary<string, object?> state,
    AgentRegistry registry,
    ExperienceResolver resolver) =>
{
    var viability = resolver.Evaluate(capability);
    if (!viability.Viable)
        return Results.Conflict(new { error = viability.Reason, capability });

    var response = await registry.ExecuteAsync(capability, state);
    return response is null
        ? Results.NotFound(new { error = $"No agent provides capability '{capability}'." })
        : Results.Ok(response);
});

app.Run();
