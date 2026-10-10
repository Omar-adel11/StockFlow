import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';
export const supplierService={
 async getAll(search=null){const url=new URL(`${baseUrl}/api/Suppliers`);if(search?.trim())url.searchParams.set('search',search.trim());return getWithAuth(url.toString());},
 async getById(id){return getWithAuth(`${baseUrl}/api/Suppliers/${id}`);},
 async create(p){return postWithAuth(`${baseUrl}/api/Suppliers`,p);},
 async update(id,p){return putWithAuth(`${baseUrl}/api/Suppliers/${id}`,p);},
 async delete(id){return delWithAuth(`${baseUrl}/api/Suppliers/${id}`);}
};