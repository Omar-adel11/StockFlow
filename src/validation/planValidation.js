export function validatePlanForm(formData) {

    const name = formData.get('name');
    const price = formData.get('price');
    const billingCycle = formData.get('billingCycle');
    const description = formData.get('description');

    const errors = {};

    
    const fields = [
        ['name', name, 100, true],
        ['description', description, 500, false],
    ];

    fields.forEach(([field, value, maxLength, required]) => {

        const text = typeof value === 'string' ? value.trim() : '';

        if (required && !text) {
            errors[field] = `${field} is required`;
        }
        else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    const priceValue = parseFloat(price);
    if (price === null || price === '' || Number.isNaN(priceValue)) {
        errors.price = 'price is required';
    }
    else if (priceValue < 0) {
        errors.price = 'price must be zero or greater';
    }

    if (!billingCycle) {
        errors.billingCycle = 'billingCycle is required';
    }

    return errors;
}