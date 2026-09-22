import { get, postWithAuth, putWithAuth, delWithAuth, baseUrl } from '../api/apiClient.js';
import * as session from '../sessions/session.js';

const plansEndpoint = `${baseUrl}/api/Plans`;


export async function fetchPlans() {
    return await get(plansEndpoint);
}

export async function createPlan(data) {
    return await postWithAuth(plansEndpoint, data);
}

export async function updatePlan(id, data) {
    return await putWithAuth(`${plansEndpoint}/${id}`, data);
}

export async function deletePlan(id) {
    return await delWithAuth(`${plansEndpoint}/${id}`);
}