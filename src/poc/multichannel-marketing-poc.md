This updated specification is designed for an AI coding agent to build a high-performance Proof of Concept (POC). It incorporates **Controller-based APIs**, the **Repository Pattern** with **EF Core**, and **persistent Testcontainers** to optimize your development loop.

***

# POC Specification: Multi-Channel Marketing Platform (High-Scale)

## 1. Project Overview & Goals
* **Objective:** Build a high-concurrency marketing engine to test system behavior under Load, Stress, and Soak conditions.
* **Scale:** 1,000 Tenants, 10 Million Campaign rows.
* **Key Focus:** Data persistence across test runs, performance monitoring via DB-stored metrics, and structured (Controller-based) API design.

## 2. Technical Stack
* **Runtime:** .NET 10
* **Database:** PostgreSQL (with EF Core 10)
* **Caching/Rate Limiting:** Redis
* **Infrastructure:** Testcontainers for .NET (Postgres & Redis)
* **Load Testing:** k6
* **Patterns:** Controller-based Web API + Repository Pattern

## 3. Project Structure
The agent must strictly follow this directory layout:
```text
src/
└── MarketingPoc/
    ├── Source/
    │   ├── Controllers/         // Controller-based API endpoints
    │   ├── Data/                // AppDbContext & Migrations
    │   ├── Models/              // Entities (Tenant, Campaign, TestResult)
    │   ├── Repositories/        // Repository Interfaces & Implementations
    │   ├── Services/            // Business Logic (Seeder, CampaignProcessor)
    │   ├── Program.cs           // DI Registration & Middleware
    │   └── appsettings.json
    │   └── MarketingPoc.csproj
    └── Tests/
        ├── Integration/         // XUnit + Testcontainers (Persistent Data Setup)
        ├── Load/                // k6 Script files (.js)
        └── MarketingPoc.Tests.csproj
```

## 4. Database Schema & Persistence Strategy

### **Entities**
* **Tenant:** `Id`, `Name`, `RateLimit`.
* **Campaign:** `Id`, `TenantId` (Indexed), `Channel`, `Content`, `Status`, `ScheduledAt`.
* **TestResult:** `Id`, `TestType` (Load/Soak), `StartTime`, `EndTime`, `RequestCount`, `ErrorCount`, `P95Latency`, `AvgCpuUsage`.

### **Testcontainer Persistence**
To avoid re-seeding 10M rows on every run, the Testcontainer setup in the `Tests/` project must:
1.  **Map a Host Volume:** Bind a local folder (e.g., `./.pgdata`) to the container's `/var/lib/postgresql/data`.
2.  **Reusable Instance:** Configure the container with `.WithCleanUp(false)` and a fixed host port to allow the data to persist between debugger restarts.

## 5. Core Implementation Details

### **A. Data Access (EF Core + Repository)**
* Implement `ICampaignRepository` and `ITenantRepository`.
* Use **EF Core Compiled Queries** for the `GetCampaignsByTenant` method to ensure high-speed retrieval.
* Ensure the `TestResultRepository` can handle async writes of performance metrics without blocking the main execution path.

### **B. High-Speed Seeding**
* Create a `SeederService` using `Npgsql`'s `BinaryImporter` (`COPY` command) to bypass EF Core overhead during the initial 10M row insertion.
* The seeder should check if data already exists (due to persistence) before starting.

### **C. Test Result Logging**
* Create a middleware or a background service that captures k6/System metrics and persists them to the `TestResults` table in PostgreSQL at the end of every test session.

## 6. Testing Scenarios (k6)

The AI agent should generate a `load-test.js` script that targets the following:
* **Endpoint:** `GET /api/tenants/{id}/campaigns`
* **Logic:** * **Load Test:** 500 VUs for 5 minutes.
    * **Stress Test:** Ramp up from 0 to 2000 VUs over 2 minutes.
    * **Soak Test:** 200 VUs for 30 minutes to check for memory leaks.

## 7. AI Agent Instructions (The "Prompt")

> *“Build a .NET 10 Web API using the **Controller-based** approach. Use **EF Core** with the **Repository Pattern** for all database interactions. Implement a high-performance seeder using Npgsql Binary Import to populate 10 million rows across 1,000 tenants into PostgreSQL.
> 
> **Crucial Requirement:** In the Integration Test project, configure **Testcontainers** for PostgreSQL using a **persistent volume mapping** to a local directory so that the 10M rows are not lost when the container stops.
> 
> Create an additional controller `TestObservationController` to save and retrieve performance results (latency, throughput) into a `TestResults` table. Ensure the folder structure follows the `src/MarketingPoc/Source` and `src/MarketingPoc/Tests` convention.”*

---

### **Success Criteria**
1.  **No Duplicate Seeding:** Second run of the application detects existing data in the persistent Testcontainer.
2.  **Clean Repository Abstraction:** Logic is decoupled from the Controllers via Repositories.
3.  **Observability:** Performance metrics from k6 runs are successfully stored in the PostgreSQL `TestResults` table.