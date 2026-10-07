namespace AgentWeb.Contracts;

public sealed record AgentDependency(
    string Capability,
    bool Required = true);

public sealed record AgentCapability(
    string Id,
    string Description,
    NavigationHint? Navigation = null,
    IReadOnlyList<AgentDependency>? Dependencies = null);

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

public interface ICapabilityInvoker
{
    Task<AgentResponse?> InvokeAsync(
        string capability,
        IReadOnlyDictionary<string, object?>? state = null);
}

public sealed record AgentRequest(
    string Capability,
    IReadOnlyDictionary<string, object?>? State = null,
    ICapabilityInvoker? Capabilities = null);

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
