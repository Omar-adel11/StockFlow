export function ValidateLoginForm(formData) {
    const email = formData.get('email');
    const password = formData.get('password');

    const errors = {};

    const fields = [
        ['email', email, 200],
        ['password', password, 100]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
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

export function ValidateRegisterForm(formData) {
    const name = formData.get('name');
    const email = formData.get('email');
    const password = formData.get('password');
    const confirmPassword = formData.get('confirmPassword');
    const file = formData.get('file');
    const BusinessName = formData.get('BusinessName');
    const PhoneNumber = formData.get('PhoneNumber');

    const errors = {};

    const fields = [
        ['name', name, 100],
        ['email', email, 200],
        ['password', password, 100],
        ['confirmPassword', confirmPassword, 100],
        ['PhoneNumber', phoneNumber, 20],
        ['BusinessName', businessName, 200]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    if (
        !errors.email &&
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
    ) {
        errors.email = 'Please enter a valid email address';
    }

    ValidatePassword(errors, password);

    if (
        !errors.confirmPassword && !errors.password &&
        password !== confirmPassword
    ) {
        errors.confirmPassword = 'Passwords do not match';
    }

    if (file instanceof File && file.name) {
        if (!file.type.startsWith('image/')) {
            errors.file = 'File must be an image';
        } else if (file.size > 10 * 1024 * 1024) {
            errors.file = 'Image must be 10 MB or smaller';
        }
    }

    return errors;
}

export function ValidateForgetPasswordForm(formData) {
    const email = formData.get('email');
    const errors = {};

    const fields = [
        ['email', email, 200]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
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

export function ValidateChangePasswordForm(formData) {
    const password = formData.get('password');
    const newPassword = formData.get('newPassword');
    const confirmNewPassword = formData.get('confirmNewPassword');

    const errors = {};
    const fields = [
        ['password', password, 100],
        ['newPassword', newPassword, 100],
        ['confirmNewPassword', confirmNewPassword, 100]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    ValidatePassword(errors, newPassword);

    if (!errors.newPassword && !errors.confirmNewPassword && newPassword !== confirmNewPassword) {
        errors.confirmNewPassword = 'Passwords do not match';
    }

    return errors;
}

export function ValidateOtpForm(formData) {
    const email = sessionStorage.getItem('email');
    const otp = formData.get('otp');
    const errors = {};

    if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
        errors.email = 'Please enter a valid email address';
    }

    if (typeof otp !== 'string' || !otp.trim()) {
        errors.otp = 'Verification code is required';
    } else if (!/^\d{6}$/.test(otp.trim())) {
        errors.otp = 'Verification code must be 6 digits';
    }

    return errors;
}

export function ValidateResetPasswordForm(formData) {
    const newPassword = formData.get('newPassword');
    const confirmNewPassword = formData.get('confirmNewPassword');

    const errors = {};
    const fields = [
        ['newPassword', newPassword, 100],
        ['confirmNewPassword', confirmNewPassword, 100]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    ValidatePassword(errors, newPassword);

    if (!errors.newPassword && !errors.confirmNewPassword && newPassword !== confirmNewPassword) {
        errors.confirmNewPassword = 'Passwords do not match';
    }

    return errors;
}

function ValidatePassword(errors, password) {
    if (!password) return;
    if (password.trim().length < 8) {
        errors.password = errors.password || 'Password must be at least 8 characters';
        errors.newPassword = errors.newPassword || 'Password must be at least 8 characters';
    }
    if (!/[A-Z]/.test(password)) {
        errors.password = errors.password || 'Password must contain at least one capital letter';
        errors.newPassword = errors.newPassword || 'Password must contain at least one capital letter';
    }
    if (!/\d/.test(password)) {
        errors.password = errors.password || 'Password must contain at least one number';
        errors.newPassword = errors.newPassword || 'Password must contain at least one number';
    }
    if (!/[^A-Za-z0-9]/.test(password)) {
        errors.password = errors.password || 'Password must contain at least one special character';
        errors.newPassword = errors.newPassword || 'Password must contain at least one special character';
    }
}