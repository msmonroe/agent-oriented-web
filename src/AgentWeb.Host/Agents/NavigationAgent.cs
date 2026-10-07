using AgentWeb.Contracts;

namespace AgentWeb.Host.Agents;

public sealed record NavigationItem(string Label, string Intent);
public sealed record NavigationModel(IReadOnlyList<NavigationItem> Items);

public sealed class NavigationAgent(
    AgentRegistry registry,
    ExperienceResolver resolver) : ISiteAgent
{
    public AgentManifest Manifest { get; } = new(
        "navigation",
        "Navigation Agent",
        "Builds navigation from viable capabilities advertised by registered agents.",
        [new("site.navigation", "Compose site navigation")]);

    public NavigationModel Build()
    {
        var items = registry.GetCapabilities()
            .Where(capability => capability.Navigation is { Visible: true })
            .Where(capability => resolver.Evaluate(capability.Id).Viable)
            .OrderBy(capability => capability.Navigation!.Priority)
            .Select(capability => new NavigationItem(capability.Navigation!.Label, capability.Id))
            .ToList();

        return new NavigationModel(items);
    }

    public Task<AgentResponse> ExecuteAsync(AgentRequest request)
    {
        var navigation = Build();
        return Task.FromResult(new AgentResponse(
            request.Capability,
            [new("navigation", new Dictionary<string, object?> { ["items"] = navigation.Items })]));
    }
}
