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
}
