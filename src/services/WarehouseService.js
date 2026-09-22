import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';

export const warehouseService = {
  async getAll() {
    return getWithAuth(`${baseUrl}/api/Warehouses`);
  },

  async getById(id) {
    return getWithAuth(`${baseUrl}/api/Warehouses/${id}`);
  },

  async create(warehousePayload) {
    return postWithAuth(`${baseUrl}/api/Warehouses`, warehousePayload);
  },

  async update(id, warehousePayload) {
    return putWithAuth(`${baseUrl}/api/Warehouses/${id}`, warehousePayload);
  },

  async delete(id) {
    return delWithAuth(`${baseUrl}/api/Warehouses/${id}`);
  }
};