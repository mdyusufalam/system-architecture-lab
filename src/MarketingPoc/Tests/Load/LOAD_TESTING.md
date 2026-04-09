# Load Testing Guide for MarketingPoc

This guide explains how to run load tests against the MarketingPoc API and store the results in the database.

## Prerequisites

1. **k6** installed on your system
   - Download from: https://k6.io/docs/getting-started/installation/
   - Verify installation: `k6 version`

2. **MarketingPoc API** running
   - Default URL: `http://localhost:5069`
   - Database must be seeded with campaign data

3. **PostgreSQL** running with test data
   - 1,000+ tenants
   - 10M+ campaigns

## Available Load Tests

### 1. Load Test with Results Capture (`load-test-with-results.js`)
- **Purpose**: Runs a realistic load test and captures results in the database
- **Duration**: 2 minutes
- **Virtual Users**: 10
- **Metrics Captured**:
  - Total requests
  - Error count
  - P95 latency
  - Average latency
  - Response time distribution
  - Status code distribution

### 2. Original Load Test (`load-test.js`)
- **Scenarios**: Load, Stress, and Soak testing
- **Load**: 500 VUs for 5 minutes
- **Stress**: Ramping from 0 to 2000 VUs
- **Soak**: 200 VUs for 30 minutes (long-running)

## Quick Start

### Option 1: Run with PowerShell Script

```powershell
cd src/MarketingPoc
.\Tests\Load\run-load-test.ps1
```

### Option 2: Run with Bash Script

```bash
cd src/MarketingPoc
bash Tests/Load/run-load-test.sh
```

### Option 3: Manual k6 Execution

```bash
# Load test with results capture
k6 run --vus 10 --duration 2m Tests/Load/load-test-with-results.js

# With custom API URL
k6 run --vus 10 --duration 2m -e BASE_URL=http://localhost:5069 Tests/Load/load-test-with-results.js

# Original load tests
k6 run Tests/Load/load-test.js

# Run only specific scenario
k6 run --scenario load Tests/Load/load-test.js
```

## Test Parameters

### Virtual Users (VUs)
```bash
k6 run --vus 50 Tests/Load/load-test-with-results.js
```

### Duration
```bash
# 5 minutes
k6 run --duration 5m Tests/Load/load-test-with-results.js

# 30 seconds
k6 run --duration 30s Tests/Load/load-test-with-results.js
```

### Custom API URL
```bash
k6 run -e BASE_URL=http://your-server:5069 Tests/Load/load-test-with-results.js
```

## Viewing Test Results

### Via API Endpoints

**Get all test results:**
```bash
curl http://localhost:5069/api/tests
```

**Get results for specific test:**
```bash
curl "http://localhost:5069/api/tests?testType=Load%20Test"
```

**Get specific test result:**
```bash
curl http://localhost:5069/api/tests/1
```

### Via pgAdmin

1. Access pgAdmin: http://localhost:5050
2. Login: `admin@marketingpoc.com` / `admin123`
3. Navigate to: `Servers > MarketingPoc DB > Databases > marketingpoc > Schemas > public > Tables > TestResults`
4. View data to see all captured test results with metrics

### Via SQL Query

```sql
-- All test results
SELECT * FROM "TestResults" ORDER BY "StartTime" DESC LIMIT 20;

-- Test statistics
SELECT 
    "TestType",
    COUNT(*) as total_tests,
    AVG("P95Latency") as avg_p95_latency,
    MAX("P95Latency") as max_p95_latency,
    AVG("ErrorCount") as avg_errors,
    SUM("RequestCount") as total_requests
FROM "TestResults"
GROUP BY "TestType"
ORDER BY MAX("StartTime") DESC;

-- Performance trends
SELECT 
    DATE("StartTime") as test_date,
    "TestType",
    AVG("P95Latency") as avg_latency,
    AVG(CAST("ErrorCount" AS FLOAT) / "RequestCount") as error_rate
FROM "TestResults"
GROUP BY DATE("StartTime"), "TestType"
ORDER BY test_date DESC;
```

## Understanding Metrics

### P95 Latency (95th Percentile)
- The response time below which 95% of requests complete
- Lower is better, typical targets: < 500ms

### Error Rate
- Percentage of failed requests
- Calculated as: `(ErrorCount / RequestCount) * 100`
- Typical target: < 1%

### Request Count
- Total number of HTTP requests made during test
- Calculated based on duration and virtual users

### Status Codes
- Distribution of HTTP response codes (200, 500, etc.)
- Helps identify error patterns

## Example Test Scenarios

### Light Load Test (Dev Verification)
```bash
k6 run --vus 5 --duration 30s Tests/Load/load-test-with-results.js
```

### Standard Load Test
```bash
k6 run --vus 10 --duration 2m Tests/Load/load-test-with-results.js
```

### Heavy Load Test
```bash
k6 run --vus 50 --duration 5m Tests/Load/load-test-with-results.js
```

### Extended Soak Test
```bash
k6 run --vus 20 --duration 30m Tests/Load/load-test-with-results.js
```

## Performance Thresholds

The tests define success criteria:

```javascript
thresholds: {
  'http_req_duration': [
    'p(95)<500',     // 95% of requests complete in < 500ms
    'p(99)<1000'     // 99% of requests complete in < 1000ms
  ],
  'http_req_failed': ['rate<0.1']  // Error rate < 10%
}
```

If thresholds are exceeded, the test still completes but marks results as failed.

## Troubleshooting

### Connection Refused
```
Error: dial tcp 127.0.0.1:5069: connect: connection refused
```
**Solution**: Ensure the API is running with `dotnet run --project src/MarketingPoc/Source/MarketingPoc.csproj`

### Test Results Not Saving
```
✗ Failed to save test result: 400
```
**Solution**: Verify the API URL is correct and the API is running. Check application logs for validation errors.

### k6 Not Found
```
k6: command not found
```
**Solution**: Install k6 from https://k6.io/docs/getting-started/installation/

### Database Connection Issues
```
Error: invalid_datasource_definition
```
**Solution**: Ensure PostgreSQL is running and the database is seeded with data.

## Comparing Test Results

To compare performance across multiple test runs:

```sql
-- Compare two recent test runs
SELECT 
    "TestType",
    "StartTime",
    "RequestCount",
    "ErrorCount",
    "P95Latency",
    AVG("P95Latency") OVER (ORDER BY "StartTime" DESC LIMIT 2 OFFSET 1) as prev_p95,
    ROUND(100.0 * ("P95Latency" - LAG("P95Latency") OVER (ORDER BY "StartTime" DESC)) / LAG("P95Latency") OVER (ORDER BY "StartTime" DESC), 2) as latency_change_percent
FROM "TestResults"
ORDER BY "StartTime" DESC
LIMIT 2;
```

## Best Practices

1. **Run tests when traffic is minimal** to avoid impacting production
2. **Start with low VU counts** and gradually increase to find breaking points
3. **Monitor database performance** during tests (check pgAdmin Monitoring)
4. **Store results regularly** for trend analysis
5. **Document test conditions** (API version, database state, etc.) in test names
6. **Use consistent test durations** for fair comparisons
7. **Baseline performance** before making changes

## Advanced: Custom Test Scripts

Create your own k6 test by copying and modifying `load-test-with-results.js`:

```javascript
// Modify the request pattern
function runRequest() {
  // Change this to test different endpoints
  const response = http.get(`${BASE_URL}/api/tenants/1/campaigns?page=1&pageSize=5`);
  // ... rest of code
}

// Adjust virtual users and duration
export const options = {
  vus: 25,
  duration: '3m'
};
```

See k6 documentation: https://k6.io/docs/

## Support

For k6 documentation and examples:
- Official Docs: https://k6.io/docs/
- GitHub: https://github.com/grafana/k6
- Community: https://community.k6.io/
