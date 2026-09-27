import { baseUrl, getWithAuth, putWithAuth } from '../api/apiClient.js';

const endpoint = `${baseUrl}/api/saasadmin/business-owners`;

export async function getBusinessOwners(search = null) {
  const url = new URL(endpoint);
  if (search?.trim()) url.searchParams.set('search', search.trim());
  return getWithAuth(url.toString());
}

export async function updateBusinessOwnerStatus(id, isActive, planId = null) {
  return putWithAuth(`${endpoint}/${id}/status`, { isActive, planId });
}
