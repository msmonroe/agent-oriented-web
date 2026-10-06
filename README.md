# Agent-Oriented Web

An experiment in building a web application whose capabilities are supplied by discoverable agents rather than hard-coded into the host.

## Core hypothesis

> Adding an agent should be capable of adding functionality to the site without requiring feature-specific changes to the host application.

The host owns rendering, security boundaries, transport, and generic interaction primitives. Agents advertise capabilities. Other agents and the experience layer can discover those capabilities at runtime.

## v0.1

The first vertical slice contains:

- **Agent Registry** — discovers the capabilities registered with the host.
- **Content Agent** — advertises the initial Home, About, and Services capabilities.
- **Navigation Agent** — builds navigation from capability metadata rather than hard-coded menu items.
- **Generic React Host** — renders navigation and content without knowing which agents supplied them.

```text
Browser
   |
Generic React Host
   |
ASP.NET Core Host
   |
Agent Registry
   |
   +-- ContentAgent
   |      +-- content.home
   |      +-- content.about
   |      +-- content.services
   |
   +-- NavigationAgent
          +-- site.navigation
```

## Architectural invariant

The host must not require feature-specific code changes when a new agent is added.

Our first acceptance test:

1. Run the site with ContentAgent and NavigationAgent.
2. Observe: `Home | About | Services`.
3. Add an EstimateAgent advertising `estimate.project` and a visible navigation hint.
4. Register/deploy that agent.
5. Observe: `Home | About | Services | Get an Estimate`.
6. No React navigation code is changed.

The next test will go further: selecting **Get an Estimate** must allow the new agent to supply the experience needed to perform that capability without adding an Estimate-specific page to the host.

## Project structure

```text
src/
  AgentWeb.Host/
    Agents/
      AgentContracts.cs
      AgentRegistry.cs
      ContentAgent.cs
      NavigationAgent.cs
    Program.cs

  agent-web-ui/
    src/
      main.jsx
      styles.css
```

## Run the prototype

### API

Requires the .NET 10 SDK.

```bash
cd src/AgentWeb.Host
dotnet run --urls http://localhost:5000
```

### UI

Requires Node.js.

```bash
cd src/agent-web-ui
npm install
npm run dev
```

The UI defaults to `http://localhost:5000` for the API. Override it with `VITE_API_URL` if needed.

## What this is not

This is not a traditional website with a chatbot attached to it.

It is an experiment in treating agents as installable application capabilities. The long-term goal is for agents to advertise their identity, capabilities, inputs, outputs, UI requirements, permissions, and events through a common contract.

## Roadmap

- [x] Define a minimal agent manifest.
- [x] Build an in-process agent registry.
- [x] Generate navigation from discovered capabilities.
- [x] Add a generic React host.
- [ ] Add EstimateAgent without changing navigation code.
- [ ] Define a declarative UI schema.
- [ ] Let EstimateAgent compose its own experience.
- [ ] Move agents out of process so capabilities can be added without rebuilding the host.
- [ ] Add agent discovery and health/lifecycle handling.
- [ ] Add permissions and policy enforcement.
- [ ] Explore A2A/MCP/A2UI compatibility.
- [ ] Add contextual navigation based on visitor intent.

## Status

Experimental. The architecture is intentionally small so each step can prove or falsify the core idea.
