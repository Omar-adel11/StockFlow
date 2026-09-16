import { checkOtp } from '../services/authService.js';
import { ValidateOtpForm } from '../validation/authValidation.js';

const form = document.getElementById('otp-form');
const submitBtn = document.getElementById('submit-btn');
const formStatus = document.getElementById('form-status');

// Pre-fill email input if saved in session storage
const savedEmail = sessionStorage.getItem('email');


form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const formData = new FormData(form);
    const errors = ValidateOtpForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const email = sessionStorage.getItem('email');
    const otp = formData.get('otp');

    const data = { email, otp };

    submitBtn.disabled = true;
    formStatus.textContent = 'Verifying code...';

    try {
        const resetToken = await checkOtp(data);
        sessionStorage.setItem('email', email);
        sessionStorage.setItem('resetToken', typeof resetToken === 'string' ? resetToken : (resetToken.resetToken || resetToken.token));

        window.location.href = 'reset-password.html';
    } catch (error) {
        console.error('OTP ERROR:', error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
});