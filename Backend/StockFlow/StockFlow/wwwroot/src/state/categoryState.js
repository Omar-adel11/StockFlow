

let categories = [];

export function setCategories(newCategories) {
    categories = newCategories;
}

export function getCategories() {
    return categories;
}

export function getCategoryById(id) {
    return categories.find(cat => cat.id === Number(id));
}

export function addCategoryToState(category) {
    categories.unshift(category);
}

export function updateCategoryInState(updatedCategory) {
    const index = categories.findIndex(cat => cat.id === updatedCategory.id);
    if (index !== -1) {
        categories[index] = updatedCategory;
    }
}

export function removeCategoryFromState(id) {
    categories = categories.filter(cat => cat.id !== Number(id));
}