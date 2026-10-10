// src/services/categoryService.js
import { getWithAuth, postWithAuth, putWithAuth, delWithAuth, baseUrl } from "../api/apiClient.js";
import * as session from '../sessions/session.js';
const categoryEndpoint = `${baseUrl}/api/Categories`;
export async function getAllCategories(search = null) {
  const url = new URL(categoryEndpoint);
  if (search?.trim()) url.searchParams.set('search', search.trim());
  return getWithAuth(url.toString(), session.getAccessToken());
}
export async function createCategory(data){ return postWithAuth(categoryEndpoint,data,session.getAccessToken()); }
export async function updateCategory(id,data){ return putWithAuth(`${categoryEndpoint}/${id}`,data,session.getAccessToken()); }
export async function deleteCategory(id){ return delWithAuth(`${categoryEndpoint}/${id}`,session.getAccessToken()); }