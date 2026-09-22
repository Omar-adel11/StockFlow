import { baseUrl, getWithAuth, postWithAuth, putWithAuth } from '../api/apiClient.js';

export const salesOrderService = {
  async getAll() {
    return getWithAuth(`${baseUrl}/api/SalesOrders`);
  },

  async getById(id) {
    return getWithAuth(`${baseUrl}/api/SalesOrders/${id}`);
  },

  async create(payload) {
    return postWithAuth(`${baseUrl}/api/SalesOrders`, payload);
  },

  async fulfillOrder(id) {
    return putWithAuth(`${baseUrl}/api/SalesOrders/${id}/fulfill`, {});
  },

  async cancelOrder(id) {
    return putWithAuth(`${baseUrl}/api/SalesOrders/${id}/cancel`, {});
  }
};

export default salesOrderService;