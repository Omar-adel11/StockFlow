import { forgetPassword } from '../services/authService.js';
import { ValidateForgetPasswordForm } from '../validation/authValidation.js';

const form = document.getElementById('forgot-password-form');
const submitBtn = document.getElementById('submit-btn');
const formStatus = document.getElementById('form-status');

form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const formData = new FormData(form);
    const errors = ValidateForgetPasswordForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const email = formData.get('email');

    submitBtn.disabled = true;
    formStatus.textContent = 'Sending verification code...';

    try {
        sessionStorage.setItem('email', email);
        const result = await forgetPassword(email);
        formStatus.textContent = typeof result === 'string' ? result : 'Verification code sent successfully.';
        setTimeout(() => {
            window.location.href = 'otp.html';
        }, 1000);
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});