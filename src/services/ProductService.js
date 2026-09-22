import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';

export const productService = {
  async getAll() {
    return getWithAuth(`${baseUrl}/api/Products`);
  },

  async getById(id) {
    return getWithAuth(`${baseUrl}/api/Products/${id}`);
  },

  async create(productPayload) {
    return postWithAuth(`${baseUrl}/api/Products`, productPayload);
  },

  async update(id, productPayload) {
    return putWithAuth(`${baseUrl}/api/Products/${id}`, productPayload);
  },

  async delete(id) {
    return delWithAuth(`${baseUrl}/api/Products/${id}`);
  }
};