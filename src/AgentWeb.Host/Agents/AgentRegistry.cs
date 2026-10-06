namespace AgentWeb.Host.Agents;

public sealed class AgentRegistry(IEnumerable<ISiteAgent> agents)
{
    private readonly IReadOnlyList<ISiteAgent> _agents = agents.ToList();

    public IReadOnlyList<AgentManifest> GetManifests() =>
        _agents.Select(agent => agent.Manifest).ToList();

    public IEnumerable<AgentCapability> GetCapabilities() =>
        _agents.SelectMany(agent => agent.Manifest.Capabilities);
}
