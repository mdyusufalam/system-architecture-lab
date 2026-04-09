# Test Results API Examples in PowerShell
# This script demonstrates how to interact with the test results API

param(
    [string]$BaseUrl = "http://localhost:5069"
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Test Results API Examples" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Create a sample test result
Write-Host "1. Creating a sample test result..." -ForegroundColor Yellow
$testPayload = @{
    testType = "Manual PowerShell Test"
    startTime = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    endTime = (Get-Date).AddMinutes(2).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    requestCount = 5000
    errorCount = 25
    p95Latency = 425.50
    avgCpuUsage = 0
}

$jsonPayload = $testPayload | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/tests" `
        -Method Post `
        -Headers @{ "Content-Type" = "application/json" } `
        -Body $jsonPayload

    $result = $response.Content | ConvertFrom-Json
    $testId = $result.id
    
    Write-Host "✓ Created test result with ID: $testId" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Cyan
    $result | ConvertTo-Json | Write-Host
} catch {
    Write-Host "✗ Failed to create test result: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Get all test results
Write-Host "2. Fetching all test results..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/tests" -Method Get
    $results = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Retrieved $(($results | Measure-Object).Count) test results" -ForegroundColor Green
    $results | Format-Table -AutoSize
} catch {
    Write-Host "✗ Failed to fetch test results: $_" -ForegroundColor Red
}

Write-Host ""

# Test 3: Filter test results by type
Write-Host "3. Filtering test results by type..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest `
        -Uri "$BaseUrl/api/tests?testType=$([Uri]::EscapeDataString('Manual PowerShell Test'))" `
        -Method Get
    
    $filtered = $response.Content | ConvertFrom-Json
    Write-Host "✓ Found $(($filtered | Measure-Object).Count) results matching filter" -ForegroundColor Green
    $filtered | Format-Table -AutoSize
} catch {
    Write-Host "✗ Failed to filter results: $_" -ForegroundColor Red
}

Write-Host ""

# Test 4: Get specific test result
if ($testId) {
    Write-Host "4. Getting specific test result (ID: $testId)..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl/api/tests/$testId" -Method Get
        $specific = $response.Content | ConvertFrom-Json
        
        Write-Host "✓ Retrieved test result:" -ForegroundColor Green
        $specific | Format-List
        
        Write-Host ""
        Write-Host "Analysis:" -ForegroundColor Cyan
        Write-Host "  Success Rate: $($specific.successRate)%"
        Write-Host "  Duration: $($specific.durationSeconds) seconds"
        Write-Host "  P95 Latency: $($specific.p95Latency)ms"
    } catch {
        Write-Host "✗ Failed to get test result: $_" -ForegroundColor Red
    }
}

Write-Host ""

# Test 5: Display SQL query for statistics
Write-Host "5. SQL Query for Test Statistics..." -ForegroundColor Yellow
Write-Host "Run this in your database to analyze test results:" -ForegroundColor Cyan

$sqlQuery = @"
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
"@

Write-Host $sqlQuery -ForegroundColor Green

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Test complete!" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. View all results: Invoke-WebRequest '$BaseUrl/api/tests' | ConvertFrom-Json"
Write-Host "  2. Run load test: .\Tests\Load\run-load-test.ps1"
Write-Host "  3. View results in pgAdmin: http://localhost:5050"
Write-Host ""
