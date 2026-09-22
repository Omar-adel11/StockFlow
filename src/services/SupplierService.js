import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';

export const supplierService = {
  async getAll() {
    return getWithAuth(`${baseUrl}/api/Suppliers`);
  },

  async getById(id) {
    return getWithAuth(`${baseUrl}/api/Suppliers/${id}`);
  },

  async create(supplierPayload) {
    return postWithAuth(`${baseUrl}/api/Suppliers`, supplierPayload);
  },

  async update(id, supplierPayload) {
    return putWithAuth(`${baseUrl}/api/Suppliers/${id}`, supplierPayload);
  },

  async delete(id) {
    return delWithAuth(`${baseUrl}/api/Suppliers/${id}`);
  }
};