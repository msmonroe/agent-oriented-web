namespace AgentWeb.Host.Agents;

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
                new(true, "Get an Estimate", "primary", 40))
        ]);

    public Task<AgentResponse> ExecuteAsync(AgentRequest request)
    {
        return Task.FromResult(new AgentResponse(
            request.Capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "CAPABILITY PROVIDED BY ESTIMATE AGENT" }),
                new("heading", new Dictionary<string, object?> { ["text"] = "Project Estimate" }),
                new("text", new Dictionary<string, object?>
                {
                    ["text"] = "This entire experience was supplied by EstimateAgent. The host contains no estimate-specific route or page."
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
            ]));
    }
}
