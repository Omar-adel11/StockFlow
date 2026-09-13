import { get, post, put, del, baseUrl } from '../api/apiClient.js';

const plansEndpoint = `${baseUrl}/api/Plans`;


export async function fetchPlans() {
    return await get(plansEndpoint);
}

export async function createPlan(data) {
    return await post(plansEndpoint, data);
}

export async function updatePlan(id, data) {
    return await put(`${plansEndpoint}/${id}`, data);
}

export async function deletePlan(id) {
    return await del(`${plansEndpoint}/${id}`);
}