import { baseUrl, getWithAuth, postWithAuth, putWithAuth, delWithAuth } from '../api/apiClient.js';
export const productService={
 async getAll(search=null){const url=new URL(`${baseUrl}/api/Products`);if(search?.trim())url.searchParams.set('search',search.trim());return getWithAuth(url.toString());},
 async getById(id){return getWithAuth(`${baseUrl}/api/Products/${id}`);},
 async create(p){return postWithAuth(`${baseUrl}/api/Products`,p);},
 async update(id,p){return putWithAuth(`${baseUrl}/api/Products/${id}`,p);},
 async delete(id){return delWithAuth(`${baseUrl}/api/Products/${id}`);}
};