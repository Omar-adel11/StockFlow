import { getWithAuth, postWithAuth, putWithAuth, delWithAuth, baseUrl } from "../api/apiClient.js";
const customerEndpoint=`${baseUrl}/api/Customers`;
export async function getAllCustomers(search=null){const url=new URL(customerEndpoint);if(search?.trim())url.searchParams.set('search',search.trim());return getWithAuth(url.toString());}
export async function getCustomerById(id){return getWithAuth(`${customerEndpoint}/${id}`);}
export async function createCustomer(data){return postWithAuth(customerEndpoint,data);}
export async function updateCustomer(id,data){return putWithAuth(`${customerEndpoint}/${id}`,data);}
export async function deleteCustomer(id){return delWithAuth(`${customerEndpoint}/${id}`);}
export const customerService={getAll:getAllCustomers,getById:getCustomerById,create:createCustomer,update:updateCustomer,delete:deleteCustomer};