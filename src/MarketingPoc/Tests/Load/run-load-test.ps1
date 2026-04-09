# Load Testing Script for MarketingPoc
# Usage: .\run-load-test.ps1 [options]

param(
    [int]$VUs = 10,
    [string]$Duration = "2m",
    [string]$BaseUrl = "http://localhost:5069",
    [string]$TestType = "load-test-with-results.js",
    [switch]$Help
)

# Colors for output
$ColorInfo = 'Cyan'
$ColorSuccess = 'Green'
$ColorError = 'Red'
$ColorWarning = 'Yellow'

function Write-Info { Write-Host "ℹ $args" -ForegroundColor $ColorInfo }
function Write-Success { Write-Host "✓ $args" -ForegroundColor $ColorSuccess }
function Write-Error_ { Write-Host "✗ $args" -ForegroundColor $ColorError }
function Write-Warning_ { Write-Host "⚠ $args" -ForegroundColor $ColorWarning }

function Show-Help {
@"
Load Testing Script for MarketingPoc

Usage: .\run-load-test.ps1 [options]

Options:
    -VUs <int>              Number of virtual users (default: 10)
    -Duration <string>      Test duration (default: 2m)
                           Examples: 30s, 1m, 5m, 30m
    -BaseUrl <string>      API base URL (default: http://localhost:5069)
    -TestType <string>     Test type: load-test-with-results.js, load-test.js
                           (default: load-test-with-results.js)
    -Help                  Display this help message

Examples:
    # Standard load test
    .\run-load-test.ps1

    # Custom VUs and duration
    .\run-load-test.ps1 -VUs 50 -Duration 5m

    # Heavy load test against custom URL
    .\run-load-test.ps1 -VUs 100 -Duration 10m -BaseUrl http://your-server:5069

    # Run original load test scenarios
    .\run-load-test.ps1 -TestType load-test.js

"@
    exit 0
}

if ($Help) {
    Show-Help
}

# Check if k6 is installed
Write-Info "Checking for k6 installation..."
$k6Version = k6 version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error_ "k6 is not installed or not in PATH"
    Write-Host "Please install k6 from: https://k6.io/docs/getting-started/installation/"
    exit 1
}

Write-Success "k6 found: $k6Version"

# Check if test file exists
$testFile = "Tests/Load/$TestType"
if (-not (Test-Path $testFile)) {
    Write-Error_ "Test file not found: $testFile"
    exit 1
}

Write-Success "Test file found: $testFile"

# Check if API is accessible
Write-Info "Checking API connectivity to $BaseUrl..."
try {
    $null = Invoke-WebRequest "$BaseUrl/api/tests" -TimeoutSec 3 -ErrorAction SilentlyContinue
    Write-Success "API is accessible at $BaseUrl"
} catch {
    Write-Warning_ "Could not reach API at $BaseUrl"
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y") {
        Write-Error_ "Test cancelled"
        exit 1
    }
}

# Display test configuration
Write-Host ""
Write-Info "========== TEST CONFIGURATION =========="
Write-Host "  Virtual Users (VUs): $VUs"
Write-Host "  Duration: $Duration"
Write-Host "  API URL: $BaseUrl"
Write-Host "  Test File: $testFile"
Write-Host "========================================"
Write-Host ""

# Run the test
Write-Info "Starting k6 load test..."
Write-Host ""

& k6 run `
    --vus $VUs `
    --duration $Duration `
    -e BASE_URL="$BaseUrl" `
    $testFile

$testExit = $LASTEXITCODE

Write-Host ""
if ($testExit -eq 0) {
    Write-Success "Load test completed successfully!"
} else {
    Write-Warning_ "Load test completed with status code: $testExit"
}

# Display next steps
Write-Host ""
Write-Info "Next steps:"
Write-Host "  1. View test results via API:"
Write-Host "     curl $BaseUrl/api/tests"
Write-Host ""
Write-Host "  2. View results in pgAdmin:"
Write-Host "     http://localhost:5050"
Write-Host ""
Write-Host "  3. Read full documentation:"
Write-Host "     Get-Content Tests/Load/LOAD_TESTING.md"
Write-Host ""

exit $testExit
