# Connector State Manager with Durable Entity

This project implements a durable entity in Azure Functions (Isolated Worker) to manage the state of a connector (`Normal`, `Faulted`, `Maintenance`).

It follows RESTful API design and integrates with external systems like the TruckMate plugin for fault detection and message handling control.

---

## 🔧 Features

- Durable Entity to persist connector state using Azure Durable Task
- Enum-based state (`ConnectorState`)
- HTTP API to get/set state
- Retry logic when reading entity state
- Integration plan for TruckMate plugin
- Fault detection on 502 errors or "access violation"
- Manual reset endpoint with function-level auth

---

## 📁 Project Structure

```
/DurableEntityTest
├── ConnectorStateEntity.cs         # Durable entity logic
├── ConnectorStateFunctions.cs     # HTTP API endpoints
├── ConnectorState.cs              # Enum definition
├── StateUpdateRequest.cs          # DTO for update requests
└── ...
```

---

## 🚀 API Endpoints

### 🔹 Set State
`POST /api/connector/{connectorId}/state`

**Request Body:**
```json
{
  "state": "Faulted"
}
```

### 🔹 Get State
`GET /api/connector/{connectorId}/state`

**Response:**
```
Connector abc-123 state is 'Faulted'.
```

### 🔹 (Planned) Reset State
To be secured via function-level authorization.

---

## 🧪 Testing

Use [Postman](https://www.postman.com/) or curl:

```bash
curl -X POST http://localhost:7071/api/connector/abc-123/state      -H "Content-Type: application/json"      -d '{ "state": "Faulted" }'
```

---

## 🛠 Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools (for local testing)
- Azure Storage Emulator or real Storage Account (for entity state backend)

---

## 📝 Notes

- Entity state is checked before processing messages.
- Faulted environments cause messages to be abandoned.
- Future improvements: throttle faulting logic (e.g., N failures in T seconds).

---

## 📄 License

MIT License. See `LICENSE` file for details.