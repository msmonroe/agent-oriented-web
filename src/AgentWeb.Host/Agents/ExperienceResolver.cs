using AgentWeb.Contracts;

namespace AgentWeb.Host.Agents;

public sealed record CapabilityViability(
    string Capability,
    bool Viable,
    string? Reason);

public sealed class ExperienceResolver(AgentRegistry registry)
{
    public CapabilityViability Evaluate(string capability) =>
        Evaluate(capability, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    public IReadOnlyList<CapabilityViability> EvaluateAll() =>
        registry.GetCapabilities()
            .Select(capability => Evaluate(capability.Id))
            .ToList();

    private CapabilityViability Evaluate(
        string capabilityId,
        HashSet<string> path)
    {
        var provider = registry.Resolve(capabilityId);
        if (provider is null)
            return new(capabilityId, false, "No provider is registered.");

        var capability = provider.Manifest.Capabilities.First(candidate =>
            string.Equals(candidate.Id, capabilityId, StringComparison.OrdinalIgnoreCase));

        if (!path.Add(capabilityId))
            return new(capabilityId, false, "A required dependency cycle was detected.");

        foreach (var dependency in capability.Dependencies ?? [])
        {
            if (!dependency.Required)
                continue;

            var dependencyResult = Evaluate(
                dependency.Capability,
                new HashSet<string>(path, StringComparer.OrdinalIgnoreCase));

            if (!dependencyResult.Viable)
                return new(
                    capabilityId,
                    false,
                    $"Required dependency '{dependency.Capability}' is not viable: {dependencyResult.Reason}");
        }

        return new(capabilityId, true, null);
    }
}
