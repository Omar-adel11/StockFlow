import * as authService from '../services/authService.js';
import * as authValidation from '../validation/authValidation.js';
import * as session from '../sessions/session.js';

const form = document.getElementById('signup-form');
const submitBtn = document.getElementById('submit-btn');
const formStatus = document.getElementById('form-status');

form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const formData = new FormData(form);
    const errors = authValidation.ValidateRegisterForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    submitBtn.disabled = true;
    formStatus.textContent = 'Registering...';

    try {
        const result = await authService.register(formData);
        session.setSession(result);
        window.location.href = 'home.html';
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});