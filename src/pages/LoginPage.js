import * as authService from '../services/authService.js';
import * as authValidation from '../validation/authValidation.js';
import * as session from '../sessions/session.js'; 

const form = document.getElementById('login-form');
const submitBtn = document.getElementById('submit-btn');
const formStatus = document.getElementById('form-status');

form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const formData = new FormData(form);
    const errors = authValidation.ValidateLoginForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const data = {
        email: formData.get('email'),
        password: formData.get('password')
    };

    submitBtn.disabled = true;
    formStatus.textContent = 'Logging in...';

    try {
        const result = await authService.login(data);
        session.setSession(result);

        const token = result?.token || session.getAccessToken();
        let role = '';
        try {
            const payload = JSON.parse(
                atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))
            );
            role =
                payload.role ||
                payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                '';
        } catch {
            // If the token cannot be decoded, keep the normal application route.
        }

        window.location.href =
            role === 'SaasAdmin' ? 'adminPanel.html' : 'dashboard.html';
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});