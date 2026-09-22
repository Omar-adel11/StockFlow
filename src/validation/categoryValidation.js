

export function ValidateCategoryForm(formData) {
    const name = formData.get('name');
    const description = formData.get('description');

    const errors = {};

    const nameText = typeof name === 'string' ? name.trim() : '';
    if (!nameText) {
        errors.name = 'Category name is required.';
    } else if (nameText.length > 100) {
        errors.name = 'Name must be 100 characters or fewer.';
    }

    const descText = typeof description === 'string' ? description.trim() : '';
    if (descText.length > 500) {
        errors.description = 'Description must be 500 characters or fewer.';
    }

    return errors;
}