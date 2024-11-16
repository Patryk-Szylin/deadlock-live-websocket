
# WebSocket Kafka Consumer Project Setup

## Prerequisites

Ensure you have the following installed:
- **.NET SDK** (version 6.0 or later)
- **Docker** and **Docker Compose**
- A Kafka instance running locally or in Docker.

## Setting Up the Environment

### Step 1: Clone the Repository

Clone the project repository to your local machine:
```bash
git clone <repository_url>
cd <project_directory>
```

### Step 3: Configure the Application

Ensure the following configuration exists in your `appsettings.json`:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

### Step 4: Run the Application

Restore dependencies, build the project, and run it:
```bash
dotnet restore
dotnet build
dotnet run
```

### Step 5: Access WebSocket Endpoint

The WebSocket server listens on `ws://localhost:8000/ws`. Clients can connect to subscribe to match updates.

## API Documentation

The application exposes the following functionality:

- **Subscribe to Match Updates:**
  Clients send a WebSocket message with the following JSON format:
  ```json
  {
    "MatchId": "example-match-id"
  }
  ```

- **Kafka Messages:**
  Messages consumed from Kafka are broadcast to all WebSocket clients subscribed to the corresponding `MatchId`.

## Notes
- Ensure Kafka is running before starting the application.