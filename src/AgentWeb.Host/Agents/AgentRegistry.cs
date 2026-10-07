using AgentWeb.Contracts;

namespace AgentWeb.Host.Agents;

public sealed record CapabilityDependencyStatus(
    string Capability,
    bool Required,
    bool Available,
    string? ProviderAgentId);

public sealed record CapabilityGraphNode(
    string Capability,
    string Description,
    string ProviderAgentId,
    string ProviderAgentName,
    IReadOnlyList<CapabilityDependencyStatus> Dependencies);

public sealed class AgentRegistry(IEnumerable<ISiteAgent> agents) : ICapabilityInvoker
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

    public IReadOnlyList<CapabilityGraphNode> GetCapabilityGraph() =>
        _agents
            .SelectMany(agent => agent.Manifest.Capabilities.Select(capability =>
                new CapabilityGraphNode(
                    capability.Id,
                    capability.Description,
                    agent.Manifest.Id,
                    agent.Manifest.Name,
                    (capability.Dependencies ?? [])
                        .Select(dependency =>
                        {
                            var provider = Resolve(dependency.Capability);
                            return new CapabilityDependencyStatus(
                                dependency.Capability,
                                dependency.Required,
                                provider is not null,
                                provider?.Manifest.Id);
                        })
                        .ToList())))
            .ToList();

    public Task<AgentResponse?> ExecuteAsync(
        string capability,
        IReadOnlyDictionary<string, object?>? state = null) =>
        InvokeAsync(capability, state);

    public async Task<AgentResponse?> InvokeAsync(
        string capability,
        IReadOnlyDictionary<string, object?>? state = null)
    {
        var agent = Resolve(capability);
        return agent is null
            ? null
            : await agent.ExecuteAsync(new AgentRequest(capability, state, this));
    }
}
