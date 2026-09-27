import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';

const endpoint = `${baseUrl}/api/Team`;

export const teamService = {
  async getMembers(search = null) {
    const url = new URL(`${endpoint}/members`);
    if (search?.trim()) url.searchParams.set('search', search.trim());
    return getWithAuth(url.toString());
  },
  async getInvites() {
    return getWithAuth(`${endpoint}/invites`);
  },
  async invite(payload) {
    return postWithAuth(`${endpoint}/invite`, payload);
  },
  async cancelInvite(id) {
    return delWithAuth(`${endpoint}/invite/${id}`);
  },
  async updateRole(id, newRole) {
    return putWithAuth(`${endpoint}/members/${id}/role`, { newRole });
  },
  async removeMember(id) {
    return delWithAuth(`${endpoint}/members/${id}`);
  }
};
