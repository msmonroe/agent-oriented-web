# Agent-Oriented Web

An experiment in building a web application whose capabilities are supplied by discoverable agents rather than hard-coded into the host.

## Core hypothesis

> Adding an agent should be capable of adding functionality to the site without requiring feature-specific changes to the host application.

The host owns rendering, security boundaries, transport, and generic interaction primitives. Agents advertise capabilities. The registry resolves capabilities to providers, and agents return declarative experiences that the generic React host renders.

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
   +-- ContentAgent
   |      +-- content.home
   |      +-- content.about
   |      +-- content.services
   |
   +-- EstimateAgent
          +-- estimate.project
```

Navigation is also capability-derived. Neither React nor NavigationAgent contains an Estimate-specific menu item.

## Acceptance tests

### Experiment #1: capability-derived navigation

1. Add EstimateAgent advertising `estimate.project`.
2. Give that capability a visible navigation hint.
3. Register the agent.
4. Observe `Get an Estimate` in navigation.
5. Do not change React navigation or NavigationAgent.

**Status: passed.**

### Experiment #2: agent-provided experience

1. Select `Get an Estimate`.
2. The generic endpoint asks AgentRegistry for the provider of `estimate.project`.
3. AgentRegistry resolves EstimateAgent.
4. EstimateAgent returns generic experience components.
5. React renders heading, text, input, select, and button components.
6. Program.cs, routing, and React contain no Estimate-specific page.

**Status: implemented for local verification.**

## Run

Requires the .NET 9 SDK and Node.js.

API:

```bash
cd src/AgentWeb.Host
dotnet run --urls http://localhost:5000
```

UI:

```bash
cd src/agent-web-ui
npm install
npm run dev
```

Open the Vite URL, normally `http://localhost:5173`.

## Important limitation

Agents are still registered in-process in `Program.cs`. Adding a new agent therefore still requires rebuilding/restarting the host. The next architectural milestone is external discovery so an agent can be added without compiling the host.

The current experience component model is intentionally tiny and proprietary. It is a proving scaffold, not a proposed standard. We intend to evaluate A2UI/A2A/MCP compatibility rather than unnecessarily inventing competing protocols.

## Roadmap

- [x] Minimal agent manifest
- [x] In-process agent registry
- [x] Capability-derived navigation
- [x] Generic React renderer
- [x] EstimateAgent adds navigation without frontend changes
- [x] Registry resolves a capability to its provider
- [x] Agent supplies a declarative experience
- [ ] Submit actions back to the owning agent
- [ ] External/runtime agent discovery
- [ ] Agent lifecycle and health
- [ ] Permissions and policy enforcement
- [ ] A2A/MCP/A2UI compatibility
- [ ] Agent dependency/capability graph
- [ ] Contextual experience composition

## Status

Experimental. Each milestone is designed to prove or falsify the central claim rather than hide it behind framework machinery.
