import { resetPassword } from '../services/authService.js';
import { ValidateResetPasswordForm } from '../validation/authValidation.js';

const form = document.getElementById('reset-password-form');
const submitBtn = document.getElementById('submit-btn');
const formStatus = document.getElementById('form-status');

form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const formData = new FormData(form);
    const errors = ValidateResetPasswordForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const email = sessionStorage.getItem('email');
    const resetToken = sessionStorage.getItem('resetToken');

    if (!email || !resetToken) {
        formStatus.textContent = 'Reset session expired. Please request a new verification code.';
        return;
    }

    const newPassword = formData.get('newPassword');
    const confirmNewPassword = formData.get('confirmNewPassword');

    const data = {
        email: email,
        resetToken: resetToken,
        password: newPassword,
        confirmPassword: confirmNewPassword
    };

    submitBtn.disabled = true;
    formStatus.textContent = 'Resetting password...';

    try {
        const result = await resetPassword(data);
        formStatus.textContent = typeof result === 'string' ? result : 'Password reset successfully!';

        sessionStorage.removeItem('email');
        sessionStorage.removeItem('resetToken');

        setTimeout(() => {
            window.location.href = 'Login.html';
        }, 1500);
    } catch (error) {
        console.error('RESET PASSWORD ERROR:', error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});