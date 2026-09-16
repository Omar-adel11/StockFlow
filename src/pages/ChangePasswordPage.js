import * as authService from '../services/authService.js';
import { ValidateChangePasswordForm } from "../validation/authValidation.js";

const form = document.getElementById('change-password-form');
const submitBtn = document.getElementById('submit-btn');
const formStatus = document.getElementById('form-status');

form.addEventListener('submit', async (event) => {
    event.preventDefault();
    
    const formData = new FormData(form);
    const errors = ValidateChangePasswordForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const data = {
        password: formData.get('password'),
        newPassword: formData.get('newPassword')
    };

    const token = sessionStorage.getItem('token');
    if (!token) {
        window.location.href = 'Login.html';
        return;
    }

    submitBtn.disabled = true;
    formStatus.textContent = 'Updating password...';

    try {
        await authService.changePassword(data, token);
        formStatus.textContent = 'Password changed successfully!';
        setTimeout(() => {
            window.location.href = 'admin.html';
        }, 1500);
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});