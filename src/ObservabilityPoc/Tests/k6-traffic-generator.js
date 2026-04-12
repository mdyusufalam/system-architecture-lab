import http from 'k6/http';
import { sleep, check } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 }, // Ramp up to 20 users
    { duration: '1m', target: 20 },  // Hold at 20 users
    { duration: '10s', target: 0 },  // Ramp down to 0 users
  ],
};

const BASE_URL = 'http://localhost:5000'; // Make sure this matches the API port

export default function () {
  // Hit the fast endpoint most often
  const resFast = http.get(`${BASE_URL}/WeatherForecast/fast`);
  check(resFast, { 'fast status was 200': (r) => r.status == 200 });
  
  sleep(1);

  // Occasionally hit the slow endpoint
  if (Math.random() > 0.7) {
    const resSlow = http.get(`${BASE_URL}/WeatherForecast/slow`);
    check(resSlow, { 'slow status was 200': (r) => r.status == 200 });
  }

  // Rarely hit the error endpoint, so we don't completely saturate the logs
  if (Math.random() > 0.9) {
    const resError = http.get(`${BASE_URL}/WeatherForecast/error`);
    check(resError, { 'error status was 500': (r) => r.status == 500 });
  }
}
