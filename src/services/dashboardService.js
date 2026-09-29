import { baseUrl, getWithAuth } from '../api/apiClient.js';

export async function getDashboardSummary() {
  return getWithAuth(baseUrl + '/api/Dashboard/summary');
}
