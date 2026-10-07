using AgentWeb.Contracts;

namespace AgentWeb.Host.Agents;

public sealed class AgentRegistry(IEnumerable<ISiteAgent> agents)
{
    private readonly IReadOnlyList<ISiteAgent> _agents = agents.ToList();

    public IReadOnlyList<AgentManifest> GetManifests() =>
        _agents.Select(agent => agent.Manifest).ToList();

    public IEnumerable<AgentCapability> GetCapabilities() =>
        _agents.SelectMany(agent => agent.Manifest.Capabilities);

    public ISiteAgent? Resolve(string capability) =>
        _agents.FirstOrDefault(agent =>
            agent.Manifest.Capabilities.Any(candidate =>
                string.Equals(candidate.Id, capability, StringComparison.OrdinalIgnoreCase)));

    public async Task<AgentResponse?> ExecuteAsync(
        string capability,
        IReadOnlyDictionary<string, object?>? state = null)
    {
        var agent = Resolve(capability);
        return agent is null
            ? null
            : await agent.ExecuteAsync(new AgentRequest(capability, state));
    }
}
