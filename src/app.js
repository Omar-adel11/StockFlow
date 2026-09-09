import { validateContactForm } from './validation/contactValidation.js';
import { sendContactMessage } from './services/contactService.js';

const form = document.getElementById('contact-form');
const formStatus = document.getElementById('form-status');
const submitButton = document.querySelector('.btn-submit');

form.addEventListener('submit',  async(event) => {
    event.preventDefault();
    
    const formData = new FormData(form);
    const errors = validateContactForm(formData);

    const hasErrors = Object.keys(errors).length > 0;
    if (hasErrors) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }
    const data = {
        name : formData.get('name'),
        email : formData.get('email'),
        subject : formData.get('subject'),
        message : formData.get('message')
    };
    submitButton.disabled = true;
    formStatus.textContent = 'sending...';        


    try{
        const result = await sendContactMessage(data);
        if (result.success) {
            formStatus.textContent = result.message;
            form.reset();
        } else {
            formStatus.textContent = result.message;
            submitButton.disabled = false;
        }
        
    }catch(error)
    {
        console.error(error);
        formStatus.textContent = 'Something went wrong. Please try again.';
        submitButton.disabled = false;
    }
        

});