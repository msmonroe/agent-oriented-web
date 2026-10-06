namespace AgentWeb.Host.Agents;

public sealed class ContentAgent : ISiteAgent
{
    public AgentManifest Manifest { get; } = new(
        "content",
        "Content Agent",
        "Provides the site's initial content capabilities.",
        [
            new("content.home", "Home content", new(true, "Home", "primary", 10)),
            new("content.about", "About content", new(true, "About", "primary", 20)),
            new("content.services", "Services content", new(true, "Services", "primary", 30))
        ]);

    public Task<AgentResponse> ExecuteAsync(AgentRequest request)
    {
        var (heading, body) = request.Capability switch
        {
            "content.about" => (
                "About This Experiment",
                "This site is a generic host. Its available experiences are discovered from registered agents."),
            "content.services" => (
                "Agent-Provided Capabilities",
                "New agents can advertise capabilities that the host can discover without hard-coding them into navigation."),
            _ => (
                "An Agent-Oriented Website",
                "The host renders the experience. Agents decide what capabilities are available.")
        };

        return Task.FromResult(new AgentResponse(
            request.Capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "GENERIC HOST / AGENT-PROVIDED EXPERIENCE" }),
                new("heading", new Dictionary<string, object?> { ["text"] = heading }),
                new("text", new Dictionary<string, object?> { ["text"] = body })
            ]));
    }
}
