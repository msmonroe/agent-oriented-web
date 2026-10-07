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
POST /api/action/{capability}
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
      ExperienceResolver.cs
      PluginAgentLoader.cs
    plugins/
    Program.cs

  AgentWeb.EstimateAgent/
    AgentWeb.EstimateAgent.csproj
    EstimateAgent.cs

  AgentWeb.SystemStatusAgent/
    AgentWeb.SystemStatusAgent.csproj
    SystemStatusAgent.cs

  AgentWeb.ProjectPlannerAgent/
    AgentWeb.ProjectPlannerAgent.csproj
    ProjectPlannerAgent.cs

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

### Experiment #5: generic agent actions

The renderer now owns generic form state for `input` and `select` primitives. A button can advertise an action capability without the renderer knowing what that capability means.

For EstimateAgent, the button advertises `estimate.project.submit`. React posts the current state bag to the generic `/api/action/{capability}` endpoint, AgentRegistry resolves the advertised action capability to EstimateAgent, and the agent returns a new declarative experience containing the estimate.

Neither the Host nor React contains Estimate-specific action handling.

**Status: passed.**

### Experiment #6: capability composition

ProjectPlannerAgent is a third runtime-discovered plugin. It provides `plan.project` and `plan.project.submit`, but it does not implement project estimation itself.

When building a plan, ProjectPlannerAgent asks the execution context for `estimate.project.submit` through the generic `ICapabilityInvoker`. AgentRegistry resolves whichever installed agent provides that capability and returns its response. The planner then composes selected results into its own experience.

ProjectPlannerAgent has no project reference to EstimateAgent and never names its type.

**Status: implemented for local verification.**

### Experiment #7: declarative capability graph

Capabilities can now declare dependencies in their manifest metadata. `plan.project.submit` declares a required dependency on `estimate.project.submit`.

The Host exposes `GET /api/capabilities`, which constructs an inspectable graph from discovered manifests. Each node reports its provider and each dependency reports whether it is currently available and which agent provides it.

This allows the runtime to inspect composition requirements before execution rather than discovering every missing dependency only after an action is invoked.

**Status: implemented for local verification.**

### Experiment #8: graph-derived experience viability

ExperienceResolver recursively evaluates whether a capability is viable. A capability is viable only when it has a provider and every required dependency is also viable. Required dependency cycles are treated as non-viable.

NavigationAgent now includes only visible capabilities that ExperienceResolver considers viable. The generic experience and action endpoints also reject non-viable capabilities before execution.

The Host exposes `GET /api/viability` for inspection.

Project Planner's visible entry capability, `plan.project`, explicitly requires `plan.project.submit`, which in turn requires `estimate.project.submit`. This creates a transitive viability chain.

With EstimateAgent installed, the entire chain is viable and Project Planner remains visible. If EstimateAgent is removed and the host is restarted, `estimate.project.submit` has no provider, which makes `plan.project.submit` non-viable, which then makes `plan.project` non-viable. Navigation therefore removes Project Planner without any Project Planner-specific rule.

**Status: implemented for local verification.**

### Experiment #9: transitive entry-point viability

The Project Planner entry capability now declares its submit capability as a required dependency. This tests whether viability propagates through multiple graph edges and whether a visible experience disappears when a transitive requirement is missing.

Expected chain:

`plan.project → plan.project.submit → estimate.project.submit`

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
- [x] Route generic actions and form state back to an owning agent
- [ ] Hot discovery / lifecycle handling
- [ ] Out-of-process agent discovery
- [ ] Permissions, trust, and policy enforcement
- [ ] A2A/MCP/A2UI compatibility
- [x] Agent dependency/capability invocation
- [x] Explicit dependency metadata and inspectable capability graph
- [x] Graph-derived capability viability
- [ ] Contextual experience composition

## Status

Experimental. Each milestone is designed to prove or falsify the central claim rather than hide it behind framework machinery.
