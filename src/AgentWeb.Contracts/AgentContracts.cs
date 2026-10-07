namespace AgentWeb.Contracts;

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

public sealed record AgentRequest(
    string Capability,
    IReadOnlyDictionary<string, object?>? State = null);

public sealed record ExperienceComponent(
    string Type,
    IReadOnlyDictionary<string, object?> Props);

public sealed record AgentResponse(
    string Capability,
    IReadOnlyList<ExperienceComponent> Components);

public interface ISiteAgent
{
    AgentManifest Manifest { get; }
    Task<AgentResponse> ExecuteAsync(AgentRequest request);
}
