import { baseUrl, getWithAuth, postWithAuth, putWithAuth } from '../api/apiClient.js';

export const purchaseOrderService = {
  async getAll() {
    return getWithAuth(`${baseUrl}/api/PurchaseOrders`);
  },

  async getById(id) {
    return getWithAuth(`${baseUrl}/api/PurchaseOrders/${id}`);
  },

  async create(payload) {
    return postWithAuth(`${baseUrl}/api/PurchaseOrders`, payload);
  },

  async receiveOrder(id) {
    return putWithAuth(`${baseUrl}/api/PurchaseOrders/${id}/receive`, {});
  },

  async cancelOrder(id) {
    return putWithAuth(`${baseUrl}/api/PurchaseOrders/${id}/cancel`, {});
  }
};

export default purchaseOrderService;