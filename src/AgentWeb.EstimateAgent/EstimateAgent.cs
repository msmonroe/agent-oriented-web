using System.Text.Json;
using AgentWeb.Contracts;

namespace AgentWeb.EstimateAgent;

public sealed class EstimateAgent : ISiteAgent
{
    public AgentManifest Manifest { get; } = new(
        "estimate",
        "Estimate Agent",
        "Provides preliminary software project estimation capabilities.",
        [
            new(
                "estimate.project",
                "Create a preliminary software project estimate",
                new(true, "Get an Estimate", "primary", 40)),
            new(
                "estimate.project.submit",
                "Submit project information for a preliminary estimate")
        ]);

    public Task<AgentResponse> ExecuteAsync(AgentRequest request) =>
        request.Capability switch
        {
            "estimate.project.submit" => Task.FromResult(BuildEstimate(request)),
            _ => Task.FromResult(BuildForm(request.Capability))
        };

    private static AgentResponse BuildForm(string capability) =>
        new(
            capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "DYNAMICALLY DISCOVERED AGENT" }),
                new("heading", new Dictionary<string, object?> { ["text"] = "Project Estimate" }),
                new("text", new Dictionary<string, object?>
                {
                    ["text"] = "EstimateAgent was loaded from the plugins folder. The host was not compiled against this agent."
                }),
                new("input", new Dictionary<string, object?>
                {
                    ["name"] = "projectDescription",
                    ["label"] = "Describe your project",
                    ["placeholder"] = "We need to modernize a legacy application..."
                }),
                new("select", new Dictionary<string, object?>
                {
                    ["name"] = "projectSize",
                    ["label"] = "Approximate project size",
                    ["options"] = new[] { "Small", "Medium", "Large", "Not sure" }
                }),
                new("button", new Dictionary<string, object?>
                {
                    ["label"] = "Get Estimate",
                    ["action"] = "estimate.project.submit"
                })
            ]);

    private static AgentResponse BuildEstimate(AgentRequest request)
    {
        var description = ReadString(request.State, "projectDescription");
        var size = ReadString(request.State, "projectSize");

        var (effort, team) = size.ToLowerInvariant() switch
        {
            "small" => ("2–4 weeks", "1–2 people"),
            "medium" => ("6–10 weeks", "2–3 people"),
            "large" => ("12–20 weeks", "3–5 people"),
            _ => ("4–16 weeks", "2–4 people")
        };

        var summary = string.IsNullOrWhiteSpace(description)
            ? "No project description was supplied."
            : $"Project: {description}";

        return new AgentResponse(
            request.Capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "GENERIC ACTION RESPONSE" }),
                new("heading", new Dictionary<string, object?> { ["text"] = "Preliminary Estimate" }),
                new("text", new Dictionary<string, object?> { ["text"] = summary }),
                new("text", new Dictionary<string, object?> { ["text"] = $"Size: {(string.IsNullOrWhiteSpace(size) ? "Not specified" : size)}" }),
                new("text", new Dictionary<string, object?> { ["text"] = $"Estimated effort: {effort}" }),
                new("text", new Dictionary<string, object?> { ["text"] = $"Suggested team: {team}" }),
                new("text", new Dictionary<string, object?>
                {
                    ["text"] = "This result came back through the generic action pipeline. Neither the host nor React contains estimate-specific submission logic."
                })
            ]);
    }

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
