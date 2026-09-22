// src/services/categoryService.js
import { getWithAuth, postWithAuth, putWithAuth, delWithAuth, baseUrl } from "../api/apiClient.js";
import * as session from '../sessions/session.js';

const categoryEndpoint = `${baseUrl}/api/Categories`;

export async function getAllCategories() {
   const token = session.getAccessToken();
    return await getWithAuth(categoryEndpoint, token);
}

export async function createCategory(data) {
    const token = session.getAccessToken();
    return await postWithAuth(categoryEndpoint, data, token);
}

export async function updateCategory(id, data) {
    const token = session.getAccessToken();
    return await putWithAuth(`${categoryEndpoint}/${id}`, data, token);
}

export async function deleteCategory(id) {
    const token = session.getAccessToken();
    return await delWithAuth(`${categoryEndpoint}/${id}`, token);
}