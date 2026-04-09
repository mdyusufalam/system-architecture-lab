#!/bin/bash
# Test Results API Examples
# This script demonstrates how to interact with the test results API

BASE_URL="${1:-http://localhost:5069}"

echo "================================"
echo "Test Results API Examples"
echo "Base URL: $BASE_URL"
echo "================================"
echo ""

# Test 1: Create a sample test result
echo "1. Creating a sample test result..."
RESULT=$(curl -s -X POST "$BASE_URL/api/tests" \
  -H "Content-Type: application/json" \
  -d '{
    "testType": "Manual Test Example",
    "startTime": "'$(date -u +%Y-%m-%dT%H:%M:%SZ)'",
    "endTime": "'$(date -u -d '+2 minutes' +%Y-%m-%dT%H:%M:%SZ)'",
    "requestCount": 5000,
    "errorCount": 25,
    "p95Latency": 425.50,
    "avgCpuUsage": 0
  }')

TEST_ID=$(echo $RESULT | grep -o '"id":[0-9]*' | grep -o '[0-9]*' | head -1)
echo "✓ Created test result with ID: $TEST_ID"
echo "Response:"
echo $RESULT | jq .
echo ""

# Test 2: Get all test results
echo "2. Fetching all test results..."
curl -s "$BASE_URL/api/tests" | jq .
echo ""

# Test 3: Filter test results by type
echo "3. Filtering test results by type..."
curl -s "$BASE_URL/api/tests?testType=Manual%20Test%20Example" | jq .
echo ""

# Test 4: Get specific test result
if [ ! -z "$TEST_ID" ]; then
    echo "4. Getting specific test result (ID: $TEST_ID)..."
    curl -s "$BASE_URL/api/tests/$TEST_ID" | jq .
    echo ""
fi

# Test 5: Calculate statistics
echo "5. Calculating test statistics..."
echo "   (In real scenario, run SQL query below)"
echo ""
echo "SQL Query:"
echo '
SELECT 
    "TestType",
    COUNT(*) as test_count,
    MIN("P95Latency") as min_p95,
    AVG("P95Latency") as avg_p95,
    MAX("P95Latency") as max_p95,
    SUM("RequestCount") as total_requests,
    SUM("ErrorCount") as total_errors,
    ROUND(100.0 * SUM("ErrorCount") / SUM("RequestCount"), 2) as error_rate_percent
FROM "TestResults"
GROUP BY "TestType"
ORDER BY COUNT(*) DESC;
'

echo ""
echo "================================"
echo "Test complete!"
echo "================================"
