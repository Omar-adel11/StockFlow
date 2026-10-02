import { get, postWithAuth, putWithAuth, delWithAuth, baseUrl } from '../api/apiClient.js';
import * as session from '../sessions/session.js';

const plansEndpoint = `${baseUrl}/api/Plans`;


function normalizePlan(plan) {
    return {
        ...plan,
        features: Array.isArray(plan?.features)
            ? plan.features.map(f => typeof f === 'string'
                ? { featureKey: '', name: f, value: '', isActive: true }
                : f)
            : []
    };
}

export async function fetchPlans() {
    const plans = await get(plansEndpoint);
    return Array.isArray(plans) ? plans.map(normalizePlan) : [];
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