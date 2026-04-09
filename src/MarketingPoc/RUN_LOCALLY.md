# Running the Marketing POC Locally

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for PostgreSQL and Redis) 
- Or PostgreSQL 17+ and Redis 7+ installed locally

## Option 1: Using Docker Compose (Recommended)

### 1. Start Infrastructure

From the `src/MarketingPoc` directory:

```bash
docker-compose up -d
```

This starts PostgreSQL and Redis containers with persistent volumes.

Verify they're running:

```bash
docker-compose ps
```

### 2. Create Database and Apply Migrations

```bash
cd src/MarketingPoc/Source
dotnet ef database update
```

### 3. Run the Application

From the repository root:

```bash
dotnet run --project src/MarketingPoc/Source/MarketingPoc.csproj
```

The API will start at `http://localhost:5000`

### 4. Seed Data (Optional)

The seeder runs automatically on startup if the `Tenants` table is empty. For a full 10M row seed, this will take several minutes.

### 5. Test the API

Use the provided `MarketingPoc.http` file in VS Code with the REST Client extension:

```bash
code src/MarketingPoc/Source/MarketingPoc.http
```

Or use curl:

```bash
# Create a test result
curl -X POST http://localhost:5000/api/tests \
  -H "Content-Type: application/json" \
  -d '{
    "testType": "Load",
    "startTime": "2026-04-08T10:00:00Z",
    "endTime": "2026-04-08T10:05:00Z",
    "requestCount": 150000,
    "errorCount": 1500,
    "p95Latency": 245.5,
    "avgCpuUsage": 65.2
  }'

# Get all test results
curl http://localhost:5000/api/tests

# Get campaigns for a tenant
curl http://localhost:5000/api/tenants/1/campaigns?page=1&pageSize=20
```

## Option 2: Local PostgreSQL and Redis

### 1. Install PostgreSQL

Download from https://www.postgresql.org/download/windows/

Create a database:

```sql
CREATE DATABASE marketingpoc;
```

### 2. Install Redis

Download from https://github.com/microsoftarchive/redis/releases (Windows) or use WSL

Start Redis:

```bash
redis-server
```

### 3. Update Connection Strings

Edit `appsettings.Development.json` if using non-default ports.

### 4. Run Application

```bash
dotnet run --project src/MarketingPoc/Source/MarketingPoc.csproj
```

## Scalar API Documentation

Once running, access the Scalar API reference:

```
http://localhost:5000/scalar/v1
```

Scalar provides an interactive, beautiful interface for exploring and testing the API endpoints.

## Running Integration Tests

From `src/MarketingPoc/Tests`:

```bash
dotnet test
```

Integration tests use the persistent Testcontainers setup to connect to a dedicated PostgreSQL instance.

## Performance Notes

- **Seeding 10M rows**: First run takes ~15-30 minutes depending on hardware
- **Subsequent runs**: Skips seeding if data persists
- **Compiled queries**: Campaign retrieval for tenants is optimized with EF Core compiled queries
- **Binary import**: Seeding uses Npgsql binary import for maximum speed

## Troubleshooting

**Connection refused error:**
- Verify PostgreSQL is running: `docker-compose ps`
- Check connection string in `appsettings.Development.json`

**Port already in use:**
- Edit `docker-compose.yml` to use different port mappings

**Database doesn't exist:**
- Run: `dotnet ef database create`

**Seeding hangs:**
- Check PostgreSQL logs: `docker-compose logs postgres`
- The 10M record seed is expected to take 15+ minutes
