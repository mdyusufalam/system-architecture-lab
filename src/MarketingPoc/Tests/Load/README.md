# Load Testing Suite

This directory contains a complete load testing framework for the MarketingPoc system, including k6 scripts, helper utilities, and comprehensive documentation.

## 📊 Quick Start (30 seconds)

### Windows (PowerShell)
```powershell
cd ..\..
.\Tests\Load\run-load-test.ps1
```

### Linux/macOS (Bash)
```bash
cd ../..
bash Tests/Load/run-load-test.sh
```

Results are **automatically saved to the database**. View them immediately:
```bash
curl http://localhost:5069/api/tests | jq .
```

## 📁 Directory Contents

| File | Purpose | Status |
|------|---------|--------|
| **load-test-with-results.js** | Primary k6 load test script with automatic result persistence | ✅ Ready |
| **run-load-test.ps1** | Windows PowerShell helper script with parameter validation | ✅ Ready |
| **run-load-test.sh** | Linux/macOS Bash equivalent | ✅ Ready |
| **LOAD_TESTING.md** | Comprehensive 400+ line guide (parameters, metrics, SQL analysis) | ✅ Complete |
| **LOAD_TEST_QUICKSTART.md** | 280+ line quick reference with common scenarios | ✅ Complete |
| **test-results-api-examples.ps1** | PowerShell examples for API usage and result querying | ✅ Ready |
| **test-results-api-examples.sh** | Bash equivalent of API examples | ✅ Ready |
| **load-test.js** | Legacy k6 script (retained for reference) | 📋 Legacy |

## 🚀 Essential Commands

### Default Test (10 VUs, 2 minutes)
```powershell
.\run-load-test.ps1
```

### Custom Parameters
```powershell
# Heavy load test: 50 virtual users for 5 minutes
.\run-load-test.ps1 -VUs 50 -Duration 5m -BaseUrl http://localhost:5069

# Quick smoke test: 3 users for 30 seconds
.\run-load-test.ps1 -VUs 3 -Duration 30s

# Soak test: 20 users for 30 minutes
.\run-load-test.ps1 -VUs 20 -Duration 30m
```

### View Test Results (3 Options)

**Option A: REST API (JSON)**
```bash
# All results
curl http://localhost:5069/api/tests

# Last 5 results
curl "http://localhost:5069/api/tests?skip=0&take=5"

# Filter by test type
curl "http://localhost:5069/api/tests?testType=Load%20Test"
```

**Option B: pgAdmin Web UI**
- URL: http://localhost:5050
- Login: `admin@marketingpoc.com` / `admin123`
- Navigate: Servers → PostgreSQL → Databases → marketingpoc → Schemas → public → Tables → TestResults

**Option C: Direct SQL Query**
```sql
-- Last 10 test results
SELECT "Id", "TestType", "StartTime", "P95Latency", "ErrorCount", "RequestCount", "SuccessRate"
FROM "TestResults"
ORDER BY "StartTime" DESC
LIMIT 10;

-- Performance trend over time
SELECT 
  DATE("StartTime") as "Date",
  AVG("P95Latency") as "Avg_P95_Latency",
  AVG("SuccessRate") as "Avg_Success_Rate",
  COUNT(*) as "Test_Count"
FROM "TestResults"
GROUP BY DATE("StartTime")
ORDER BY "Date" DESC;
```

## 📈 Common Test Scenarios

### Baseline (Development)
```powershell
.\run-load-test.ps1 -VUs 5 -Duration 1m
# Expected: p95 < 500ms, error rate < 1%
```

### Standard Load
```powershell
.\run-load-test.ps1 -VUs 10 -Duration 2m
# Expected: p95 < 500ms, error rate < 1%
```

### Stress Test
```powershell
.\run-load-test.ps1 -VUs 50 -Duration 5m
# Expected: p95 < 1000ms, error rate < 5%
```

### Soak Test (Stability)
```powershell
.\run-load-test.ps1 -VUs 20 -Duration 30m
# Expected: Consistent p95 < 600ms throughout duration
```

## 📊 Understanding Metrics

All test results include these key metrics:

| Metric | Unit | Expected | What It Means |
|--------|------|----------|---------------|
| **P95 Latency** | milliseconds | < 500ms | 95% of requests complete in this time |
| **Success Rate** | percentage | > 99% | Percentage of successful responses |
| **Error Count** | requests | < 1% | Failed or timeout requests |
| **Request Count** | total | varies | Total requests executed |
| **Avg CPU Usage** | percentage | varies | Average CPU utilization |

## 🔧 Advanced Usage

### Custom Test Type Label
```powershell
# Results will be tagged with custom label for analysis
.\run-load-test.ps1 -TestType "PerformanceOptimization"
```

### Multiple Concurrent Runs
```powershell
# Not recommended - will increase contention
# Instead, stagger runs and compare results:
.\run-load-test.ps1 -Duration 2m
# Wait 2 minutes
.\run-load-test.ps1 -VUs 50 -Duration 2m -TestType "Heavy Load"
```

### Manual k6 Execution
```bash
# Run k6 directly with custom flags (requires k6 installed)
k6 run --vus 10 --duration 2m -e BASE_URL=http://localhost:5069 load-test-with-results.js
```

## 📚 Documentation

**For getting started**: [`LOAD_TEST_QUICKSTART.md`](./LOAD_TEST_QUICKSTART.md)
- 30-second setup
- Sample output
- Common scenarios
- Performance expectations

**For deep dives**: [`LOAD_TESTING.md`](./LOAD_TESTING.md)
- Complete parameter guide
- Troubleshooting section
- SQL analysis examples
- Performance threshold definitions
- Advanced customization
- Best practices

**For API integration**: [`test-results-api-examples.ps1`](./test-results-api-examples.ps1)
- Shows how to query results programmatically
- Includes filtering examples
- SQL analysis demonstrations

## ✅ Prerequisites

- **k6**: Install from https://k6.io/docs/getting-started/installation/
- **MarketingPoc API Running**: `http://localhost:5069` (default)
- **PostgreSQL Database**: Containers running via docker-compose
- **PowerShell 5.0+** (for Windows) OR **Bash** (for Linux/macOS)

Verify k6 installation:
```bash
k6 version
```

## 🔄 How Results Are Captured

1. **Test Execution**: `load-test-with-results.js` runs the test scenario
2. **Metrics Collection**: k6 gathers detailed performance metrics during execution
3. **Result Calculation**: P95 latency, error rates, and success rates are computed
4. **Automatic Posting**: Results are automatically sent to `POST /api/tests` endpoint
5. **Database Storage**: TestObservationController stores results in PostgreSQL
6. **Immediate Query**: Results are queryable via `GET /api/tests` immediately after

## 🐛 Troubleshooting

### k6 Not Found
```
Error: k6 executable not found in PATH
```
**Solution**: Install k6 from https://k6.io/docs/getting-started/installation/

### Connection Refused
```
Error: dial tcp: lookup localhost: no such host
```
**Solution**: Verify MarketingPoc API is running on http://localhost:5069

### Test Results Not Appearing
```
curl http://localhost:5069/api/tests
# Returns empty array
```
**Solution**: Check that:
1. API response shows results were posted (script log shows "✓ Result saved")
2. Database connection is working (check PostgreSQL container is running)
3. Run test again and watch for result posting message

### Timeout Error
```
Error: timeout reading from socket
```
**Solution**: 
- Reduce VUs: `.\run-load-test.ps1 -VUs 5 -Duration 1m`
- Increase duration: Let the API warm up
- Check CPU usage on API server

## 📋 Test Parameter Reference

| Parameter | Option | Default | Example |
|-----------|--------|---------|---------|
| Virtual Users | `-VUs` | 10 | `-VUs 50` |
| Duration | `-Duration` | 2m | `-Duration 5m` |
| Base URL | `-BaseUrl` | http://localhost:5069 | `-BaseUrl http://api.prod.com` |
| Test Type | `-TestType` | Load Test - [date] | `-TestType "Stress Test"` |

## 📞 Quick Help

```powershell
# Show all options
.\run-load-test.ps1 -Help

# Show what will run (dry run)
.\run-load-test.ps1 -VUs 10 -Duration 2m
# (Script will verify parameters before executing)
```

## 🎯 Next Steps

1. **Run first test**: `.\run-load-test.ps1`
2. **View results**: `curl http://localhost:5069/api/tests`
3. **Analyze metrics**: Check P95 latency and success rate
4. **Run specialized tests**: Use CLI parameters for stress/soak tests
5. **Compare trends**: Run SQL queries to track performance over time

---

**System Status**: ✅ Production-Ready
- MarketingPoc API: Running on port 5069
- PostgreSQL Database: 10M+ campaigns seeded
- Redis Cache: Active
- pgAdmin: Accessible on port 5050
- Load Testing Infrastructure: Complete and ready to execute
