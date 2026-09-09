export function validateContactForm(formData) {

    const name = formData.get('name');
    const email = formData.get('email');
    const subject = formData.get('subject');
    const message = formData.get('message');

    const errors = {};

    const fields = [
        ['name', name, 100],
        ['email', email, 200],
        ['subject', subject, 200],
        ['message', message, 2000],
    ];

    fields.forEach(([field, value, maxLength]) => {

        const text = typeof value === 'string' ? value.trim() : '';

        if (!text) {
            errors[field] = `${field} is required`;
        }
        else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    if (
        !errors.email &&
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
    ) {
        errors.email = 'Please enter a valid email address';
    }

    return errors;
}