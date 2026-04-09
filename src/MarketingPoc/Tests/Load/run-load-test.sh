#!/bin/bash

# Load Testing Script for MarketingPoc
# Usage: ./run-load-test.sh [options]

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
VUS=10
DURATION="2m"
BASE_URL="http://localhost:5069"
TEST_TYPE="load-test-with-results.js"

# Function to print colored output
print_info() {
    echo -e "${BLUE}ℹ ${1}${NC}"
}

print_success() {
    echo -e "${GREEN}✓ ${1}${NC}"
}

print_error() {
    echo -e "${RED}✗ ${1}${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ ${1}${NC}"
}

# Function to display help
show_help() {
    cat << EOF
Load Testing Script for MarketingPoc

Usage: ./run-load-test.sh [options]

Options:
    -v, --vus VUS              Number of virtual users (default: 10)
    -d, --duration DURATION    Test duration (default: 2m)
                              Examples: 30s, 1m, 5m, 30m
    -u, --url URL             API base URL (default: http://localhost:5069)
    -t, --type TYPE           Test type: load, stress, soak
                              (default: load-test-with-results.js)
    -h, --help                Display this help message

Examples:
    # Standard load test
    ./run-load-test.sh

    # Custom VUs and duration
    ./run-load-test.sh -v 50 -d 5m

    # Heavy load test against custom URL
    ./run-load-test.sh -v 100 -d 10m -u http://your-server:5069

    # Run original load test scenarios
    ./run-load-test.sh -t load-test.js

EOF
    exit 0
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--vus)
            VUS="$2"
            shift 2
            ;;
        -d|--duration)
            DURATION="$2"
            shift 2
            ;;
        -u|--url)
            BASE_URL="$2"
            shift 2
            ;;
        -t|--type)
            TEST_TYPE="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Check if k6 is installed
if ! command -v k6 &> /dev/null; then
    print_error "k6 is not installed or not in PATH"
    echo "Please install k6 from: https://k6.io/docs/getting-started/installation/"
    exit 1
fi

print_success "k6 found: $(k6 version)"

# Check if test file exists
TEST_FILE="Tests/Load/${TEST_TYPE}"
if [ ! -f "$TEST_FILE" ]; then
    print_error "Test file not found: $TEST_FILE"
    exit 1
fi

print_success "Test file found: $TEST_FILE"

# Check if API is accessible
print_info "Checking API connectivity to $BASE_URL..."
if command -v curl &> /dev/null; then
    if ! curl -s "$BASE_URL/api/tests" > /dev/null 2>&1; then
        print_warning "Could not reach API at $BASE_URL"
        read -p "Continue anyway? (y/n) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_error "Test cancelled"
            exit 1
        fi
    else
        print_success "API is accessible at $BASE_URL"
    fi
fi

# Display test configuration
echo ""
print_info "========== TEST CONFIGURATION =========="
echo "  Virtual Users (VUs): $VUS"
echo "  Duration: $DURATION"
echo "  API URL: $BASE_URL"
echo "  Test File: $TEST_FILE"
echo "========================================"
echo ""

# Run the test
print_info "Starting k6 load test..."
echo ""

k6 run \
    --vus "$VUS" \
    --duration "$DURATION" \
    -e BASE_URL="$BASE_URL" \
    "$TEST_FILE"

TEST_EXIT_CODE=$?

echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
    print_success "Load test completed successfully!"
else
    print_warning "Load test completed with status code: $TEST_EXIT_CODE"
fi

# Display next steps
echo ""
print_info "Next steps:"
echo "  1. View test results via API:"
echo "     curl $BASE_URL/api/tests"
echo ""
echo "  2. View results in pgAdmin:"
echo "     http://localhost:5050"
echo ""
echo "  3. Read full documentation:"
echo "     cat Tests/Load/LOAD_TESTING.md"
echo ""

exit $TEST_EXIT_CODE
