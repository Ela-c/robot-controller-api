# Robot Controller API

A full-stack robot control platform built with ASP.NET Core and React. The project provides a secure, session-based API for managing users, maps, robot state, reusable command definitions, and asynchronous robot command sequences, with real-time execution updates delivered over SignalR.

This repository is designed to demonstrate practical backend engineering skills: layered architecture, policy-based authorization, asynchronous processing, API + realtime integration, and test coverage.

## Project Overview

The system models a virtual robot operating on grid-based maps. Users can:

- Authenticate via secure server-side sessions (HTTP-only cookies)
- Manage map resources and robot positioning
- Create and execute robot command definitions
- Submit multi-command robot sequences for asynchronous execution
- Monitor execution status in real time through SignalR events

The API remains the source of truth while SignalR streams live progress to connected clients.

## Video DEMO

[![Robot Controller Demo](https://img.youtube.com/vi/x4rzKIxZ7LI/maxresdefault.jpg)](https://youtu.be/x4rzKIxZ7LI)

## Feature Highlights

- Session authentication and revocation
  - Cookie-based session tokens with server-side persistence and explicit revocation support
  - Endpoints for logout and logout-from-all-devices
- Role and policy-based access control
  - Fine-grained authorization policies (`CatalogRead`, `CatalogWrite`, `UserAdmin`, `SelfOrAdmin`)
- Robot command lifecycle management
  - Create/update/delete command definitions
  - Submit command executions and query execution status
  - Cancel in-flight commands when allowed
- Asynchronous robot sequence processing
  - Queue-backed background execution service
  - Sequence state transitions and structured failure handling (`ProblemDetails`)
- Realtime updates with SignalR
  - Hub endpoint for command subscription/unsubscription
  - Live command and robot position update events
- Frontend dashboard (React + Vite)
  - API integration via Axios
  - Realtime client updates using `@microsoft/signalr`
  - Query-state management with TanStack Query
- Testing and maintainability
  - xUnit backend tests with EF Core InMemory support
  - Vitest + Testing Library setup for frontend tests

## Tech Stack

### Backend

- ASP.NET Core 8 Web API
- Entity Framework Core 8
- PostgreSQL via Npgsql
- SignalR for realtime communication
- Serilog for structured logging
- Swashbuckle / Swagger for API documentation
- BCrypt for password hashing

### Frontend

- React 19 + TypeScript
- Vite
- Tailwind CSS
- Axios
- TanStack React Query
- SignalR JavaScript client

### Testing

- xUnit (`robot-controller-api.Tests`)
- EF Core InMemory (backend test persistence)
- Vitest + Testing Library (frontend)

## Architecture Summary

```text
React Frontend (Vite)
  |
  +--> REST API (ASP.NET Core Controllers)
  |
  +--> SignalR Hub (/hubs/robot)

API Layer
  |
  +--> Service Layer (robot orchestration, sequences, domain rules)
  |
  +--> Persistence Layer (repositories + EF Core DbContext)
  |
  +--> PostgreSQL

Background Hosted Service
  |
  +--> Robot sequence queue processing
  |
  +--> Realtime notifier (SignalR)
```

## API and Realtime Endpoints

- Swagger UI (Development): `/swagger`
- REST root examples:
  - `/users`
  - `/api/maps`
  - `/api/robot/state`
  - `/api/robot-commands`
  - `/api/robot/sequences`
- SignalR hub: `/hubs/robot`
- SignalR demo page: `/signalr-demo.html`

## Getting Started

### Prerequisites

Install the following tools before running the project:

- .NET SDK 8.0+
- Node.js 20+ and npm
- PostgreSQL 14+ (or compatible recent version)
- Git

Optional but useful:

- `dotnet-ef` CLI tool for migration workflows

```bash
dotnet tool install --global dotnet-ef
```

### Clone the Repository

```bash
git clone https://github.com/Ela-c/robot-controller-api.git
cd robot-controller-api
```

### Configure Environment

1. Create a PostgreSQL database for the app.
2. Configure a connection string named `RobotConnection`.

You can set this using User Secrets (recommended for local development):

```bash
dotnet user-secrets set "ConnectionStrings:RobotConnection" "Host=localhost;Port=5432;Database=robot_controller;Username=postgres;Password=your_password"
```

### Install Dependencies

Backend:

```bash
dotnet restore
```

Frontend:

```bash
cd frontend
npm install
cd ..
```

### Apply Database Migrations

```bash
dotnet ef database update
```

### Run the Project

Run backend API (from repository root):

```bash
dotnet run
```

Default local endpoints from launch profile:

- `http://localhost:5080`
- `https://localhost:7265`

Run frontend (in a second terminal):

```bash
cd frontend
npm run dev
```

The frontend runs on:

- `http://localhost:5173`

By default, the frontend API base URL is `http://localhost:5080`.
To change it, set:

```bash
VITE_API_BASE_URL=http://localhost:5080
```

## Development Commands

### Backend

- Build: `dotnet build`
- Run tests: `dotnet test`

### Frontend

- Dev server: `npm run dev`
- Type-check: `npm run typecheck`
- Lint: `npm run lint`
- Test: `npm run test`
- Production build: `npm run build`

## Security Notes

- Authentication is cookie/session-based (not JWT bearer tokens).
- Session cookies are marked `HttpOnly` and `Secure`.
- Use HTTPS in development and production when testing authenticated flows.

## Recruiter-Facing Technical Value

This project demonstrates:

- Full-stack integration between modern React frontend and .NET backend
- Secure authentication design with server-side revocable sessions
- Realtime system design patterns (REST as source of truth + SignalR streaming)
- Asynchronous workflow orchestration using hosted background services and queue abstractions
- Clean separation of concerns across controllers, services, and data access layers
- Practical API design with role-based policies and robust response modeling

## License

No license file is currently included in this repository. Add a license if you plan to distribute or open-source this project.
