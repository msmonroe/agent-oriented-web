using AgentWeb.Contracts;

namespace AgentWeb.SystemStatusAgent;

public sealed class SystemStatusAgent : ISiteAgent
{
    public AgentManifest Manifest { get; } = new(
        "system-status",
        "System Status Agent",
        "Provides a deterministic runtime system status experience.",
        [
            new(
                "system.status",
                "Display current runtime system status",
                new(true, "System Status", "primary", 50))
        ]);

    public Task<AgentResponse> ExecuteAsync(AgentRequest request)
    {
        var process = Environment.ProcessId;
        var machine = Environment.MachineName;
        var framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        var os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        var utc = DateTimeOffset.UtcNow.ToString("u");

        return Task.FromResult(new AgentResponse(
            request.Capability,
            [
                new("eyebrow", new Dictionary<string, object?> { ["text"] = "SECOND RUNTIME-DISCOVERED PLUGIN" }),
                new("heading", new Dictionary<string, object?> { ["text"] = "System Status" }),
                new("text", new Dictionary<string, object?> { ["text"] = $"Status: Operational · Checked {utc}" }),
                new("text", new Dictionary<string, object?> { ["text"] = $"Host: {machine} · Process: {process}" }),
                new("text", new Dictionary<string, object?> { ["text"] = $"Runtime: {framework}" }),
                new("text", new Dictionary<string, object?> { ["text"] = $"OS: {os}" }),
                new("text", new Dictionary<string, object?>
                {
                    ["text"] = "This capability came from AgentWeb.SystemStatusAgent.dll. No Host or React feature code was changed to add it."
                })
            ]));
    }
}
