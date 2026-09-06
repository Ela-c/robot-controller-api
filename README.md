# robot-controller-api

## Realtime robot command updates (SignalR)

SignalR supplements the existing REST API for live updates during asynchronous command execution.

### SignalR endpoint

- `/hubs/robot`

### Hub methods

- `SubscribeToCommand(commandId)`
- `UnsubscribeFromCommand(commandId)`

### Server events

- `RobotCommandUpdated`
- `RobotPositionUpdated`

### Event payload examples

`RobotCommandUpdated`

```json
{
  "commandId": 42,
  "name": "MOVE",
	"status": "Executing",
  "createdAt": "2026-09-06T10:20:00Z",
  "startedAt": "2026-09-06T10:20:02Z",
  "completedAt": null,
  "updatedAt": "2026-09-06T10:20:02Z",
  "failureReason": null
}
```

`RobotPositionUpdated`

```json
{
  "commandId": 42,
  "x": 1,
  "y": 0,
  "timestamp": "2026-09-06T10:20:03Z"
}
```

### Realtime architecture

```text
Client
  |
  | POST command
  v
ASP.NET Core API
  |
  v
Command Queue
  |
  v
Background Worker
  |
  +------> Database
  |
  +------> IRobotUpdateNotifier
				 |
				 v
			 SignalR Hub
				 |
				 v
			 Connected Client
```

### Behavior and recovery

- REST endpoints remain the source of truth:
  - `POST /api/robot-commands`
  - `GET /api/robot-commands/{id}`
  - `POST /api/robot-commands/{id}/cancel`
- SignalR provides live updates while connected.
- If a client disconnects, it should reconnect, query REST for current status, then resubscribe.
- Hub methods are not standard REST operations and are not listed in Swagger operations.

### Demo client

A lightweight manual test page is available at:

- `/signalr-demo.html`
