import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    load: {
      executor: 'constant-vus',
      vus: 500,
      duration: '5m',
      exec: 'loadScenario'
    },
    stress: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 500 },
        { duration: '90s', target: 2000 },
        { duration: '30s', target: 0 }
      ],
      exec: 'stressScenario'
    },
    soak: {
      executor: 'constant-vus',
      vus: 200,
      duration: '30m',
      exec: 'soakScenario'
    }
  },
  thresholds: {
    'http_req_duration{scenario:load}': ['p(95)<500'],
    'http_req_duration{scenario:stress}': ['p(95)<1000'],
    'http_req_duration{scenario:soak}': ['p(95)<500']
  }
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

function runRequest() {
  const tenantId = Math.floor(Math.random() * 1000) + 1;
  const response = http.get(`${BASE_URL}/api/tenants/${tenantId}/campaigns?page=1&pageSize=20`);

  check(response, {
    'status is 200': (r) => r.status === 200,
    'body is not empty': (r) => r.body && r.body.length > 0
  });

  sleep(1);
}

export function loadScenario() {
  runRequest();
}

export function stressScenario() {
  runRequest();
}

export function soakScenario() {
  runRequest();
}
