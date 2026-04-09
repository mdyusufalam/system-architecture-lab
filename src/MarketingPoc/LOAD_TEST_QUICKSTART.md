# Load Testing Quick Start Guide

## Overview
The MarketingPoc system is now fully equipped to run load tests and automatically capture results in the database.

## What's Available

### Load Test Scripts
1. **load-test-with-results.js** - Captures detailed metrics and saves to database
2. **load-test.js** - Original comprehensive scenarios (load, stress, soak)

### Helper Scripts
- **run-load-test.ps1** - PowerShell script for Windows
- **run-load-test.sh** - Bash script for Linux/Mac

### Documentation
- **LOAD_TESTING.md** - Comprehensive guide with examples and troubleshooting

## Quick Start (30 seconds)

### Prerequisites
1. Install k6: https://k6.io/docs/getting-started/installation/
2. Ensure API is running: `dotnet run --project src/MarketingPoc/Source/MarketingPoc.csproj`
3. Ensure database is seeded with data

### Run Load Test

#### Windows (PowerShell)
```powershell
cd src/MarketingPoc
.\Tests\Load\run-load-test.ps1
```

#### Linux/Mac (Bash)
```bash
cd src/MarketingPoc
bash Tests/Load/run-load-test.sh
```

#### Manual (Any System)
```bash
k6 run --vus 10 --duration 2m -e BASE_URL=http://localhost:5069 src/MarketingPoc/Tests/Load/load-test-with-results.js
k6 run --vus 50 --duration 5m -e BASE_URL=http://localhost:5069 Tests/Load/load-test-with-results.js
```

## View Results

### Option 1: Via REST API
```bash
# Get all test results
curl http://localhost:5069/api/tests | jq .

# Get results for specific test
curl http://localhost:5069/api/tests/1 | jq .
```

### Option 2: Via pgAdmin Web UI
```
URL: http://localhost:5050
Email: admin@marketingpoc.com
Password: admin123

Navigate to: Servers > MarketingPoc DB > marketingpoc > Tables > TestResults
```

### Option 3: Direct SQL Query
```sql
-- View latest test results
SELECT * FROM "TestResults" ORDER BY "StartTime" DESC LIMIT 10;

-- View test metrics
SELECT 
    "TestType",
    COUNT(*) as test_count,
    AVG("P95Latency") as avg_p95_ms,
    AVG(CAST("ErrorCount" AS FLOAT) / "RequestCount" * 100) as avg_error_rate
FROM "TestResults"
GROUP BY "TestType";
```

## Test Report Example

After running a test, you'll see output like:

```
=== Test Results ===
Total Requests: 1,234
Total Errors: 12
Error Rate: 0.97%
P95 Latency: 487.23ms
P99 Latency: 892.15ms
Avg Latency: 234.56ms
Test Duration: 120.45s
Status Codes: {"200":1222,"500":12}
===================

✓ Test result saved with ID: 42
```

## Key Metrics Explained

| Metric | Description | Good Value |
|--------|-------------|-----------|
| **P95 Latency** | 95% of requests complete in this time | < 500ms |
| **Error Rate** | Percentage of failed requests | < 1% |
| **Total Requests** | Number of API calls made | Depends on load |
| **Status Codes** | Distribution of HTTP responses | Mostly 200s |

## Common Test Scenarios

### Development Verification (Quick Test)
```bash
.\Tests\Load\run-load-test.ps1 -VUs 5 -Duration 30s
```
- Duration: 30 seconds
- Load: 5 virtual users
- Purpose: Quick sanity check

### Standard Load Test (Recommended)
```bash
.\Tests\Load\run-load-test.ps1 -VUs 10 -Duration 2m
```
- Duration: 2 minutes
- Load: 10 virtual users
- Purpose: Regular performance validation

### Performance Benchmark
```bash
.\Tests\Load\run-load-test.ps1 -VUs 50 -Duration 5m
```
- Duration: 5 minutes
- Load: 50 virtual users
- Purpose: Establish performance baseline

### Stability Test (Soak)
```bash
.\Tests\Load\run-load-test.ps1 -VUs 20 -Duration 30m
```
- Duration: 30 minutes
- Load: 20 virtual users
- Purpose: Long-running stability test

## Test Results Structure

Each test result stored in the database contains:

```json
{
  "id": 1,
  "testType": "Load Test - 2026-04-09",
  "startTime": "2026-04-09T10:30:00Z",
  "endTime": "2026-04-09T10:32:00Z",
  "requestCount": 1234,
  "errorCount": 12,
  "p95Latency": 487.23,
  "avgCpuUsage": 0,
  "successRate": 99.03,  // Calculated automatically
  "durationSeconds": 120.45  // Calculated automatically
}
```

## Expected Performance

Based on the current infrastructure (PostgreSQL with 10M campaigns, Redis cache):

- **P95 Latency**: 300-500ms per request
- **Throughput**: ~10-20 requests/second per VU
- **Error Rate**: < 1% under normal load
- **Max Safe VUs**: 100+ for short duration

## Monitoring During Tests

### Option 1: Watch k6 Output
The test script displays real-time metrics as it runs.

### Option 2: Monitor pgAdmin
Open PostgreSQL monitoring while test is running:
```
http://localhost:5050 > Servers > MarketingPoc DB > Dashboard
```

### Option 3: Check Database During Test
```sql
-- Monitor requests in real-time
SELECT 
    COUNT(*) as total_requests,
    COUNT(CASE WHEN "ErrorCount" > 0 THEN 1 END) as error_count
FROM "TestResults"
WHERE "StartTime" > NOW() - INTERVAL '5 minutes';
```

## Troubleshooting

### k6 Not Found
```
k6: command not found
```
**Solution**: Install from https://k6.io/docs/getting-started/installation/

### API Connection Error
```
Error: dial tcp 127.0.0.1:5069: connect: connection refused
```
**Solution**: Start the API with `dotnet run --project src/MarketingPoc/Source/MarketingPoc.csproj`

### Test Results Not Saving
If you see `✗ Failed to save test result: 400`

**Check**:
1. API is running: `curl http://localhost:5069/api/tests`
2. Database connection: Check PostgreSQL is running
3. API logs for validation errors

### Timeout During Test
```
Timeout during reading attempt
```
**Solution**: 
1. Reduce VUs: `--vus 5`
2. Reduce duration: `--duration 1m`
3. Check database performance

## Next Steps

1. **Run a baseline test** to establish performance metrics
2. **Schedule regular tests** (daily/weekly) to track trends
3. **Compare results** after code changes to measure impact
4. **Archive results** for compliance/audit purposes
5. **Analyze trends** to identify performance bottlenecks

## SQL Examples for Analysis

### Recent Test Performance
```sql
SELECT * FROM "TestResults" 
ORDER BY "StartTime" DESC 
LIMIT 10;
```

### Performance Trends
```sql
SELECT 
    DATE("StartTime") as test_date,
    AVG("P95Latency") as avg_latency,
    AVG(CAST("ErrorCount" AS FLOAT) / "RequestCount") as error_rate
FROM "TestResults"
GROUP BY DATE("StartTime")
ORDER BY test_date DESC;
```

### Best vs Worst Performance
```sql
SELECT "TestType", MIN("P95Latency") as best, MAX("P95Latency") as worst
FROM "TestResults"
GROUP BY "TestType";
```

## Support & Documentation

- **Full Guide**: Read `LOAD_TESTING.md`
- **k6 Docs**: https://k6.io/docs/
- **Rest API**: http://localhost:5069/scalar/v1

---

**You're ready to test!** Run your first load test now: `.\Tests\Load\run-load-test.ps1`
