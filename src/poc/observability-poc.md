This specification is designed for an AI coding agent to build a comprehensive Observability Proof of Concept (POC). It focuses on **OpenTelemetry**, **Distributed Tracing**, **Metrics**, and **Structured Logging** using an industry-standard stack (Prometheus, Grafana, Jaeger, Seq).

***

# POC Specification: Full-Stack Observability Platform

## 1. Project Overview & Goals
* **Objective:** Implement the "Three Pillars of Observability" (Metrics, Traces, Logs) into a high-performance .NET Web API to monitor system behavior, track bottlenecks, and diagnose issues under load.
* **Scope:** Instrumenting HTTP endpoints, EF Core (PostgreSQL) database queries, Redis cache operations, and background processing tasks.
* **Key Focus:** Zero-code or low-code instrumentation via OpenTelemetry, custom business metrics, and centralized visualization.

## 2. Technical Stack
* **Runtime:** .NET 10
* **API Architecture:** Controller-based API or Minimal APIs
* **Database & Cache:** PostgreSQL (EF Core 10), Redis
* **Observability (OpenTelemetry):**
  * **Traces:** Jaeger
  * **Metrics:** Prometheus
  * **Logs:** Seq (or Elasticsearch/Kibana)
  * **Collector:** OpenTelemetry Collector (OTLP)
  * **Dashboards:** Grafana
* **Infrastructure:** Docker Compose (local hosting of the observability stack)
* **Load Generation:** k6 (to generate traffic and telemetry data)

## 3. Project Structure
The agent must follow this directory layout:
```text
src/
└── ObservabilityPoc/
    ├── Source/
    │   ├── Controllers/         // API Endpoints for generating traces
    │   ├── Services/            // Business logic generating custom metrics/spans
    │   ├── Infrastructure/      // OpenTelemetry Extensions & Setup
    │   ├── Program.cs           // Dependency Injection & OTel Registration
    │   └── ObservabilityPoc.csproj
    ├── Tests/
    │   └── k6-traffic-generator.js // Script to simulate varied traffic profiles (success, slow, errors)
    └── Docker/
        ├── docker-compose.yml   // Jaeger, Prometheus, Grafana, Seq, OTel Collector
        ├── grafana/
        │   └── provisioning/    // Pre-configured dashboards & datasources
        └── prometheus/
            └── prometheus.yml   // Scrape configs for Prometheus
```

## 4. Core Implementation Details

### **A. OpenTelemetry (OTel) Setup**
* Register `OpenTelemetry` in `Program.cs` configured for:
  * **Tracing:** Instrument ASP.NET Core, HttpClient, EF Core, and StackExchange.Redis. Export traces via OTLP.
  * **Metrics:** Instrument ASP.NET Core, HTTP runtime, and .NET runtime metrics. Export to Prometheus/OTLP.
  * **Logging:** Configure OpenTelemetry Logger Provider to export structured logs alongside traces.

### **B. Distributed Tracing**
* Add manual trace spans using `ActivitySource` in specific complex business logic flows (e.g., in `Services/`) to demonstrate how to track multi-stage operations that aren't natively instrumented.
* Ensure log statements contain `TraceId` and `SpanId` so logs can be correlated to specific request traces in Seq and Grafana.

### **C. Custom Metrics**
* Demonstrate creating custom `Meter` and `Counter<T>`, `Histogram<T>` metrics to track business-level occurrences (e.g., "orders_processed_total", "cache_miss_latency").

### **D. Infrastructure via Docker Compose**
* Include a `docker-compose.yml` that stands up:
  1. **Prometheus:** For storing metric time-series data.
  2. **Jaeger:** For receiving and displaying distributed traces.
  3. **Seq:** For structured and easily searchable application logs.
  4. **Grafana:** Pre-configured with data sources (Prometheus, Jaeger) and a basic dashboard to visualize API hit rates, latencies, and system health.
  5. **OpenTelemetry Collector:** To receive OTLP from the app and fan-out to the respective backends.

## 5. Traffic Simulation (k6)

Generate a `k6-traffic-generator.js` script that targets the API:
* **Endpoints:** Mix of fast, slow (simulated delay), and error-throwing endpoints to generate rich telemetry.
* **Logic:** Include requests that hit the cache (fast), hit the DB (slower), and occasionally fail (to generate error traces and logs).

## 6. AI Agent Instructions (The "Prompt")

> *"Build a .NET 10 Web API project specifically designed to demonstrate **Observability**. Integrate the OpenTelemetry SDK to capture Metrics, Traces, and Logs. 
> Ensure out-of-the-box instrumentation is enabled for ASP.NET Core, EF Core (PostgreSQL), and Redis. Create custom `Activity` spans and `Meter` metrics within the business logic services to show custom telemetry.
> 
> Build a corresponding `docker-compose.yml` that self-hosts Prometheus, Jaeger, Seq, and Grafana. Configure the .NET API to export OTLP data to these tools. Provide a `k6` script that generates a variety of traffic patterns (successes, slow requests, failures) so the observability dashboards populate with meaningful data."*

---

### **Success Criteria**
1. **End-to-End Visibility:** A single HTTP request to the API results in a correlated Trace (Jaeger), updated Metrics (Prometheus), and Structured Logs (Seq).
2. **Dashboard Ready:** Grafana can successfully display pre-configured dashboards of the API's performance and error rates.
3. **Correlation:** Logs automatically include `TraceId` for seamless cross-referencing between application errors and the overarching request trace.
