# Agent-Oriented Web

An experiment in building a web application whose capabilities are supplied by discoverable agents rather than hard-coded into the host.

## Core hypothesis

> Adding an agent should be capable of adding functionality to the site without requiring feature-specific changes to the host application.

The host owns rendering, security boundaries, transport, and generic interaction primitives. Agents advertise capabilities through a shared contract. The host discovers plugin assemblies at startup, the registry resolves capabilities to providers, and agents return declarative experiences that the generic React host renders.

## Current architecture

```text
Browser
   |
Generic React Renderer
   |
GET /api/experience/{capability}
   |
Agent Registry
   |
   +-- built-in ContentAgent
   |
   +-- runtime-discovered agents
          |
          +-- AgentWeb.EstimateAgent.dll
          |       +-- estimate.project
          |
          +-- AgentWeb.SystemStatusAgent.dll
                  +-- system.status
```

`AgentWeb.Host` does not reference or compile against either plugin project.

Shared types such as `ISiteAgent`, `AgentManifest`, `AgentRequest`, and `AgentResponse` live in `AgentWeb.Contracts`.

## Acceptance tests

### Experiment #1: capability-derived navigation

EstimateAgent advertises `estimate.project` with a navigation hint. NavigationAgent discovers the capability and produces **Get an Estimate** without Estimate-specific React navigation code.

**Status: passed.**

### Experiment #2: agent-provided experience

Selecting **Get an Estimate** resolves `estimate.project` to its provider. EstimateAgent returns generic heading, text, input, select, and button components. React renders those primitives without an Estimate-specific page.

**Status: passed.**

### Experiment #3: runtime plugin discovery

EstimateAgent was moved into the independent `AgentWeb.EstimateAgent` project. The compiled DLL is copied into `AgentWeb.Host/plugins`. At startup, PluginAgentLoader scans that directory and instantiates implementations of the shared `ISiteAgent` contract.

The host has no project reference to EstimateAgent and `Program.cs` never names it.

**Status: passed.**

## Project structure

```text
src/
  AgentWeb.Contracts/
    AgentWeb.Contracts.csproj
    AgentContracts.cs

  AgentWeb.Host/
    Agents/
      AgentRegistry.cs
      ContentAgent.cs
      NavigationAgent.cs
      PluginAgentLoader.cs
    plugins/
    Program.cs

  AgentWeb.EstimateAgent/
    AgentWeb.EstimateAgent.csproj
    EstimateAgent.cs

  AgentWeb.SystemStatusAgent/
    AgentWeb.SystemStatusAgent.csproj
    SystemStatusAgent.cs

  agent-web-ui/
    src/
      main.jsx
      styles.css
```

## Run the current prototype

Requires .NET 9 and Node.js.

### 1. Build the plugins

From the repository root:

```powershell
dotnet build src\AgentWeb.EstimateAgent
```

### 2. Install the plugin

```powershell
New-Item -ItemType Directory -Force src\AgentWeb.Host\plugins | Out-Null
Copy-Item src\AgentWeb.EstimateAgent\bin\Debug\net9.0\AgentWeb.EstimateAgent.dll src\AgentWeb.Host\plugins\
```

The copied DLL is intentionally ignored by Git because it is a build artifact.

### 3. Start the API

```powershell
cd src\AgentWeb.Host
dotnet run --urls http://localhost:5000
```

### 4. Start the UI

In another terminal, from the repository root:

```powershell
cd src\agent-web-ui
npm install
npm run dev
```

Open `http://localhost:5173`.

With both plugins installed, navigation should include **Get an Estimate** and **System Status**. System Status is supplied by an unrelated plugin and uses only renderer primitives that already existed before the plugin was written.

### Prove runtime composition

Stop the host, remove `src\AgentWeb.Host\plugins\AgentWeb.EstimateAgent.dll`, and restart. **Get an Estimate** should disappear.

Copy the same DLL back and restart the unchanged host. The capability should reappear.

### Experiment #4: unrelated-plugin generality test

SystemStatusAgent advertises `system.status` from a second independent assembly. It reports deterministic runtime information using only the existing generic `eyebrow`, `heading`, and `text` primitives.

No Host route, NavigationAgent logic, or React renderer code was changed to support System Status.

**Status: implemented for local verification.**

## Current limitations

Discovery currently occurs at host startup. Installing or removing a plugin requires a host restart.

The DLL loader is an intermediate experiment, not the intended final distribution or trust model. Loading arbitrary executable assemblies into the host raises isolation, versioning, security, and lifecycle concerns. A later architecture should evaluate out-of-process agents and standards-based discovery.

The current experience component model is intentionally tiny. It proves generic rendering but is not intended to become a proprietary UI standard. A2UI/A2A/MCP compatibility should be evaluated before expanding it substantially.

## Roadmap

- [x] Minimal shared agent contract
- [x] Capability-derived navigation
- [x] Generic React renderer
- [x] Capability-to-provider resolution
- [x] Agent-provided declarative experience
- [x] Runtime plugin discovery without compiling host against the plugin
- [x] Add a second unrelated plugin as a generality test
- [ ] Route generic actions and form state back to an owning agent
- [ ] Hot discovery / lifecycle handling
- [ ] Out-of-process agent discovery
- [ ] Permissions, trust, and policy enforcement
- [ ] A2A/MCP/A2UI compatibility
- [ ] Agent dependency/capability graph
- [ ] Contextual experience composition

## Status

Experimental. Each milestone is designed to prove or falsify the central claim rather than hide it behind framework machinery.
