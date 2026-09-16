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
        window.location.href = 'home.html';
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});