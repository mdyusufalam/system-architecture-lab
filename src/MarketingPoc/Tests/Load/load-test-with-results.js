import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
  scenarios: buildVuSweepScenarios([50, 60, 70, 80, 90, 100], '1m'),
  thresholds: {
    'http_req_duration': ['p(95)<500', 'p(99)<1000'],
    'http_req_failed': ['rate<0.1']
  },
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)']
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5069';
const TEST_API_URL = `${BASE_URL}/api/tests`;

function buildVuSweepScenarios(vusLevels, duration) {
  return vusLevels.reduce((scenarios, vus, index) => {
    scenarios[`vu_${vus}`] = {
      executor: 'constant-vus',
      vus,
      duration,
      exec: 'loadScenario',
      startTime: `${index}m`,
      gracefulStop: '0s',
      tags: {
        profile: `vu-${vus}`
      }
    };

    return scenarios;
  }, {});
}

function runRequest() {
  const tenantId = Math.floor(Math.random() * 1000) + 1;
  const pageSize = Math.random() < 0.5 ? 10 : 50;

  const response = http.get(
    `${BASE_URL}/api/tenants/${tenantId}/campaigns?page=1&pageSize=${pageSize}`
  );

  check(response, {
    'status is 200': (r) => r.status === 200,
    'body is not empty': (r) => r.body && r.body.length > 0,
    'response time < 500ms': (r) => r.timings.duration < 500
  });

  sleep(Math.random() * 2);
}

export function loadScenario() {
  group('Campaign API Load Test', () => {
    runRequest();
  });
}

export default function () {
  runRequest();
}

function getMetricValue(metric, key, fallback = 0) {
  if (!metric || !metric.values) {
    return fallback;
  }

  const value = metric.values[key];
  return Number.isFinite(value) ? value : fallback;
}

function roundMetric(value) {
  return Number.isFinite(value) ? Math.round(value * 100) / 100 : 0;
}

function formatMetric(value, unit = '') {
  return Number.isFinite(value) ? `${roundMetric(value)}${unit}` : 'N/A';
}

function formatDuration(durationMs) {
  if (!Number.isFinite(durationMs) || durationMs < 0) {
    return 'N/A';
  }

  return `${(durationMs / 1000).toFixed(2)}s`;
}

function getThresholdResults(metrics) {
  const results = [];

  for (const [metricName, metric] of Object.entries(metrics || {})) {
    for (const [thresholdName, thresholdResult] of Object.entries(metric.thresholds || {})) {
      results.push({
        metricName,
        thresholdName,
        ok: thresholdResult.ok === true,
        actual: thresholdResult.actual || 'n/a'
      });
    }
  }

  return results;
}

function getRunDurationMs(data) {
  if (data && data.state && Number.isFinite(data.state.testRunDurationMs)) {
    return data.state.testRunDurationMs;
  }

  return NaN;
}

export function handleSummary(data) {
  const httpReqDuration = data.metrics.http_req_duration;
  const httpReqFailed = data.metrics.http_req_failed;
  const httpReqs = data.metrics.http_reqs;
  const checksMetric = data.metrics.checks;

  const endTime = new Date();
  const testRunDurationMs = getRunDurationMs(data);
  const startTime = Number.isFinite(testRunDurationMs)
    ? new Date(endTime.getTime() - testRunDurationMs)
    : endTime;

  const totalRequests = getMetricValue(httpReqs, 'count');
  const totalHttpErrors = getMetricValue(httpReqFailed, 'passes', NaN);
  const httpErrorRate = getMetricValue(httpReqFailed, 'rate');
  const failedChecks = getMetricValue(checksMetric, 'fails');
  const passedChecks = getMetricValue(checksMetric, 'passes');
  const totalChecks = passedChecks + failedChecks;
  const p95Latency = getMetricValue(httpReqDuration, 'p(95)');
  const p99Latency = getMetricValue(httpReqDuration, 'p(99)', NaN);
  const avgLatency = getMetricValue(httpReqDuration, 'avg');
  const thresholdResults = getThresholdResults(data.metrics);
  const crossedThresholds = thresholdResults.filter((threshold) => !threshold.ok);

  const vusMax = getMetricValue(data.metrics.vus, 'max') || getMetricValue(data.metrics.vus_max, 'value') || 0;

  const testResult = {
    testType: `Load Test - ${new Date().toISOString().split('T')[0]}`,
    startTime: startTime.toISOString(),
    endTime: endTime.toISOString(),
    requestCount: Math.round(totalRequests),
    errorCount: Math.round(Number.isFinite(totalHttpErrors) ? totalHttpErrors : httpErrorRate * totalRequests),
    p95Latency: roundMetric(p95Latency),
    avgCpuUsage: 0,
    vus: Math.round(vusMax),
    scenarios: (options && options.scenarios && typeof options.scenarios.getFullExecutionRequirements === 'undefined')
      ? Object.keys(options.scenarios).join(', ')
      : 'CLI Override (default)'
  };

  console.log('\n=== Test Results ===');
  console.log(`Scenario Profiles: ${testResult.scenarios}`);
  console.log(`Total Requests: ${testResult.requestCount}`);
  console.log(`HTTP Errors: ${testResult.errorCount}`);
  console.log(`HTTP Error Rate: ${totalRequests > 0 ? (httpErrorRate * 100).toFixed(2) : '0.00'}%`);
  console.log(`Failed Checks: ${Math.round(failedChecks)} / ${Math.round(totalChecks)}`);
  console.log(`P95 Latency: ${formatMetric(testResult.p95Latency, 'ms')}`);
  console.log(`P99 Latency: ${formatMetric(p99Latency, 'ms')}`);
  console.log(`Avg Latency: ${formatMetric(avgLatency, 'ms')}`);
  console.log(`Test Duration: ${formatDuration(testRunDurationMs)}`);
  console.log(`Threshold Status: ${crossedThresholds.length === 0 ? 'PASS' : 'FAIL'}`);

  crossedThresholds.forEach((threshold) => {
    console.log(`Threshold Failed: ${threshold.metricName} ${threshold.thresholdName} (actual: ${threshold.actual})`);
  });

  console.log('===================\n');
  console.log('Posting test results to API...');

  const payload = JSON.stringify(testResult);
  const params = {
    headers: { 'Content-Type': 'application/json' }
  };

  const response = http.post(TEST_API_URL, payload, params);

  if (check(response, {
    'result posted successfully': (r) => r.status === 201,
    'response has ID': (r) => r.json('id') !== null
  })) {
    console.log(`[OK] Test result saved with ID: ${response.json('id')}`);
  } else {
    console.log(`[ERROR] Failed to save test result: ${response.status}`);
  }

  return {
    stdout: JSON.stringify(testResult, null, 2)
  };
}
