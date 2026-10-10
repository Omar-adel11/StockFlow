import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';
export const warehouseService={
 async getAll(search=null){const url=new URL(`${baseUrl}/api/Warehouses`);if(search?.trim())url.searchParams.set('search',search.trim());return getWithAuth(url.toString());},
 async getById(id){return getWithAuth(`${baseUrl}/api/Warehouses/${id}`);},
 async create(p){return postWithAuth(`${baseUrl}/api/Warehouses`,p);},
 async update(id,p){return putWithAuth(`${baseUrl}/api/Warehouses/${id}`,p);},
 async delete(id){return delWithAuth(`${baseUrl}/api/Warehouses/${id}`);}
};