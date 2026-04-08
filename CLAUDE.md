# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

An HR assistant chatbot sample application built on RavenDB 7.x AI Agents, .NET Aspire, ASP.NET Core 10, and React. It demonstrates RavenDB features: AI Agents, vector search, time series, attachments, counters, and document expiration.

## Build & Run Commands

### Full application (via Aspire)

```bash
aspire run
```

Requires env vars: `SAMPLES_HR_OPENAI_API_KEY`, `SAMPLES_HR_RAVEN_LICENSE`

### Backend only

```bash
dotnet build                    # build entire solution
dotnet build SamplesHR.Backend  # build backend project only
```

### Frontend only

```bash
cd sampleshr-frontend
npm ci         # install deps (prefer ci over install)
npm run build  # production build
npm start      # dev server
npm test       # run tests
```

### CI

CI runs `dotnet restore --force-evaluate && dotnet build --no-restore` for backend and `npm ci && npm run build` in `sampleshr-frontend/` for frontend. See `.github/workflows/build.yml`.

## Architecture

### Aspire Orchestration (`SamplesHR.AppHost`)
`AppHost.cs` wires everything: starts a RavenDB container (nightly 7.2.x), creates the `HRAssistant` database, launches the React frontend as an npm app, and the ASP.NET backend. The Aspire dashboard exposes a "Seed data" button that hits `POST /api/seed/all`.

### Backend (`SamplesHR.Backend`)

ASP.NET Core 10 web API. Key areas:

- **Controllers/** — `HumanResourcesAgentController` handles chat (SSE streaming via `text/event-stream`), employee lookups, document signing, and bill image serving. `SeedController` populates sample data.
- **Infrastructure/RavenDB/** — `RavenInitializer` (hosted service) sets up the DB on startup. `HumanResourcesAgentCreator` defines the RavenDB AI Agent (`hr-assistant`) with its system prompt, RQL queries (vector search, date-range filters), and tool actions (RaiseIssue, SignDocument, ReportBusinessTripExpense).
- **Infrastructure/Middleware/** — `ChatEphemeralSessionMiddleware` extracts/creates session IDs, applied only to the `/api/humanresourcesagent/chat` path.
- **Application/Usage/** — Session and global rate limiting + tracking via RavenDB time series. Rate limits are configurable via `SAMPLES_HR_MAX_GLOBAL_REQUESTS_PER_15_MINUTES` and `SAMPLES_HR_MAX_SESSION_REQUESTS_PER_30_SECONDS`.
- **Hubs/** — `UsageStatsHub` (SignalR) broadcasts real-time API usage stats to the frontend.
- **Models/RavenDBAiAgent/** — DTOs for the AI agent conversation (Conversation, Message, Reply, tool argument classes).

### Frontend (`sampleshr-frontend/`)
React 19 app (Create React App). Communicates with the backend via SSE for chat streaming and SignalR for real-time usage stats. Source in `src/`, components in `src/components/`.

### Shared project (`backend/HRAssistant`)
A separate .csproj that is not referenced by the solution — likely for auxiliary/migration tooling.

## Key Patterns

- **Central package management**: all NuGet versions in `Directory.Packages.props` (root). Use `<PackageReference>` without `Version` in individual .csproj files.
- **RavenDB AI Agent**: the agent is defined programmatically in `HumanResourcesAgentCreator.cs`, not via config files. Tool handlers are registered in the controller's `Chat` method using `conversation.Handle<T>()` and `conversation.Receive<T>()`.
- **SSE streaming**: chat responses stream as `text/event-stream` with `data:` lines, finished by an `event: final` message.
- **CORS**: configured from Aspire service discovery — frontend URL comes from `services:frontend:http:0` config key.
