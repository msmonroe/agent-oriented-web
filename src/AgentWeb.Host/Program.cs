using AgentWeb.Host.Agents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISiteAgent, ContentAgent>();
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

app.MapGet("/api/content/{intent}", (string intent) =>
{
    var content = intent switch
    {
        "content.about" => new
        {
            type = "page",
            heading = "About This Experiment",
            body = "This site is a generic host. Its available experiences are discovered from registered agents."
        },
        "content.services" => new
        {
            type = "page",
            heading = "Agent-Provided Capabilities",
            body = "New agents can advertise capabilities that the host can discover without hard-coding them into navigation."
        },
        _ => new
        {
            type = "page",
            heading = "An Agent-Oriented Website",
            body = "The host renders the experience. Agents decide what capabilities are available."
        }
    };

    return Results.Ok(content);
});

app.Run();
