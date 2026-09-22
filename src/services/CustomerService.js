import { getWithAuth, postWithAuth, putWithAuth, delWithAuth, baseUrl } from "../api/apiClient.js";

const customerEndpoint = `${baseUrl}/api/Customers`;

export async function getAllCustomers() {
    return await getWithAuth(customerEndpoint);
}

export async function getCustomerById(id) {
    return await getWithAuth(`${customerEndpoint}/${id}`);
}

export async function createCustomer(data) {
    return await postWithAuth(customerEndpoint, data);
}

export async function updateCustomer(id, data) {
    return await putWithAuth(`${customerEndpoint}/${id}`, data);
}

export async function deleteCustomer(id) {
    return await delWithAuth(`${customerEndpoint}/${id}`);
}

export const customerService = {
    getAll: getAllCustomers,
    getById: getCustomerById,
    create: createCustomer,
    update: updateCustomer,
    delete: deleteCustomer
};