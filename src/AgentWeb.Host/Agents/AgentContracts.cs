namespace AgentWeb.Host.Agents;

public sealed record AgentCapability(
    string Id,
    string Description,
    NavigationHint? Navigation = null);

public sealed record NavigationHint(
    bool Visible,
    string Label,
    string Group = "primary",
    int Priority = 100);

public sealed record AgentManifest(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<AgentCapability> Capabilities);

public interface ISiteAgent
{
    AgentManifest Manifest { get; }
}
