using System.Text.Json;
using AgentWeb.Contracts;

namespace AgentWeb.ProjectPlannerAgent;

public sealed class ProjectPlannerAgent : ISiteAgent
{
    private const string EstimateCapability = "estimate.project.submit";

    public AgentManifest Manifest { get; } = new(
        "project-planner",
        "Project Planner Agent",
        "Composes a project plan by consuming capabilities provided by other agents.",
        [
            new(
                "plan.project",
                "Create a project plan using available capabilities",
                new(true, "Project Planner", "primary", 45)),
            new(
                "plan.project.submit",
                "Generate a composed project plan")
        ]);

    public Task<AgentResponse> ExecuteAsync(AgentRequest request) =>
        request.Capability switch
        {
            "plan.project.submit" => BuildPlanAsync(request),
            _ => Task.FromResult(BuildForm(request.Capability))
        };

    private static AgentResponse BuildForm(string capability) =>
        new(
            capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "CAPABILITY COMPOSITION EXPERIMENT" }),
                new("heading", new Dictionary<string, object?> { ["text"] = "Project Planner" }),
                new("text", new Dictionary<string, object?>
                {
                    ["text"] = "This agent does not estimate projects itself. It asks the runtime for an estimation capability and composes the result into a plan."
                }),
                new("input", new Dictionary<string, object?>
                {
                    ["name"] = "projectDescription",
                    ["label"] = "Describe your project",
                    ["placeholder"] = "Replace our legacy order management system..."
                }),
                new("select", new Dictionary<string, object?>
                {
                    ["name"] = "projectSize",
                    ["label"] = "Approximate project size",
                    ["options"] = new[] { "Small", "Medium", "Large", "Not sure" }
                }),
                new("button", new Dictionary<string, object?>
                {
                    ["label"] = "Build Project Plan",
                    ["action"] = "plan.project.submit"
                })
            ]);

    private static async Task<AgentResponse> BuildPlanAsync(AgentRequest request)
    {
        if (request.Capabilities is null)
            return Error(request.Capability, "No capability runtime was supplied.");

        var estimate = await request.Capabilities.InvokeAsync(
            EstimateCapability,
            request.State);

        if (estimate is null)
            return Error(
                request.Capability,
                $"Required capability '{EstimateCapability}' is not currently available.");

        var description = ReadString(request.State, "projectDescription");
        var size = ReadString(request.State, "projectSize");

        var components = new List<ExperienceComponent>
        {
            new("eyebrow", new Dictionary<string, object?> { ["text"] = "COMPOSED FROM MULTIPLE AGENTS" }),
            new("heading", new Dictionary<string, object?> { ["text"] = "Project Plan" }),
            new("text", new Dictionary<string, object?>
            {
                ["text"] = string.IsNullOrWhiteSpace(description)
                    ? "Project scope was not specified."
                    : $"Scope: {description}"
            }),
            new("text", new Dictionary<string, object?>
            {
                ["text"] = $"Planning class: {(string.IsNullOrWhiteSpace(size) ? "Not specified" : size)}"
            }),
            new("text", new Dictionary<string, object?>
            {
                ["text"] = $"Dependency resolved at runtime: {EstimateCapability}"
            })
        };

        components.AddRange(
            estimate.Components
                .Where(component => component.Type == "text")
                .Where(component =>
                {
                    var text = component.Props.TryGetValue("text", out var value)
                        ? value?.ToString() ?? string.Empty
                        : string.Empty;
                    return text.StartsWith("Estimated effort:", StringComparison.OrdinalIgnoreCase)
                        || text.StartsWith("Suggested team:", StringComparison.OrdinalIgnoreCase);
                }));

        components.Add(new(
            "text",
            new Dictionary<string, object?>
            {
                ["text"] = "ProjectPlannerAgent never referenced EstimateAgent. It requested a capability from the runtime and composed the provider's response."
            }));

        return new AgentResponse(request.Capability, components);
    }

    private static AgentResponse Error(string capability, string message) =>
        new(
            capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "DEPENDENCY UNAVAILABLE" }),
                new("heading", new Dictionary<string, object?> { ["text"] = "Project Plan" }),
                new("text", new Dictionary<string, object?> { ["text"] = message })
            ]);

    private static string ReadString(
        IReadOnlyDictionary<string, object?>? state,
        string key)
    {
        if (state is null || !state.TryGetValue(key, out var value) || value is null)
            return string.Empty;

        return value switch
        {
            JsonElement element when element.ValueKind == JsonValueKind.String =>
                element.GetString() ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }
}
