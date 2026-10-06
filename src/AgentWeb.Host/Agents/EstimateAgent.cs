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
}
