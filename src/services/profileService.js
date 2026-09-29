import { baseUrl, getWithAuth } from '../api/apiClient.js';
import { getAccessToken } from '../sessions/session.js';

export async function getMyProfile() {
  return getWithAuth(baseUrl + '/api/Users/me');
}

export async function updateMyProfile(data) {
  const response = await fetch(baseUrl + '/api/Users/me', {
    method: 'PUT',
    headers: { Authorization: 'Bearer ' + getAccessToken() },
    body: data
  });
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.message || body?.title || 'Failed to update profile.');
  }
  return response.status === 204 ? null : response.json();
}
