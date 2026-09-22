export function validateCustomerForm(formData) {
    const errors = [];

    if (!formData.name || !formData.name.trim()) {
        errors.push("Name is required.");
    }

    if (!formData.email || !formData.email.trim()) {
        errors.push("Email is required.");
    } else {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(formData.email.trim())) {
            errors.push("Please enter a valid email address.");
        }
    }

    if (!formData.phone || !formData.phone.trim()) {
        errors.push("Phone number is required.");
    }

    return {
        isValid: errors.length === 0,
        errors
    };
}

/**
 * Assembles flat form fields into the exact JSON schema expected by ASP.NET Core API
 */
export function buildCustomerPayload(formData) {
    return {
        Name: formData.name.trim(),
        Email: formData.email.trim(),
        Phone: formData.phone.trim(),
        Addresses: [
            {
                Street: formData.addressStreet ? formData.addressStreet.trim() : "",
                City: formData.addressCity ? formData.addressCity.trim() : "",
                State: formData.addressState ? formData.addressState.trim() : "NA",
                ZipCode: formData.addressPostalCode ? formData.addressPostalCode.trim() : "",
                Country: formData.addressCountry ? formData.addressCountry.trim() : "Egypt",
                IsDefault: true
            }
        ]
    };
}