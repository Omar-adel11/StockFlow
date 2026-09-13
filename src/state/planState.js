let plans = [];

export function setPlans(newPlans) {
    plans = newPlans;
}

export function getPlans() {
    return plans;
}

export function getPlanById(id) {
    return plans.find(plan => plan.id === Number(id));
}   