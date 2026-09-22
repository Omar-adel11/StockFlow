import { baseUrl, getWithAuth, postWithAuth } from '../api/apiClient.js';

export const inventoryService = {
  async getAll() {
    return getWithAuth(`${baseUrl}/api/Inventory`);
  },

  async getByProduct(productId) {
    return getWithAuth(`${baseUrl}/api/Inventory/product/${productId}`);
  },

  async getByWarehouse(warehouseId) {
    return getWithAuth(`${baseUrl}/api/Inventory/warehouse/${warehouseId}`);
  },

  async getLowStock() {
    return getWithAuth(`${baseUrl}/api/Inventory/low-stock`);
  },

  async getStockMovements(count = 20) {
    return getWithAuth(`${baseUrl}/api/StockMovements/recent?count=${count}`);
  },

  async adjustStock(payload) {
    return postWithAuth(`${baseUrl}/api/StockMovements/adjust`, payload);
  }
};

export default inventoryService;