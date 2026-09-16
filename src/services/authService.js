import { post, baseUrl, postWithAuth } from "../api/apiClient.js";
import * as session from '../sessions/session.js';

const loginEndpoint = `${baseUrl}/api/Authentication/login`;
export async function login(data) {
    return await post(loginEndpoint, data);
}

const registerEndpoint = `${baseUrl}/api/Authentication/signup`;
export async function register(formData) {
    const response = await fetch(registerEndpoint, {
        method: 'POST',
        body: formData
    });
    
    if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new Error(
            errorData?.errors ? Object.values(errorData.errors).flat().join(' ') : 
            errorData?.message || 'Registration failed'
        );
    }
    if (response.status === 204) return null;
    return response.json();
}

const changePasswordEndpoint = `${baseUrl}/api/Authentication/change-password`;
export async function changePassword(data, token) {
    return await postWithAuth(changePasswordEndpoint, data, token);
}

const forgetPasswordEndpoint = `${baseUrl}/api/Authentication/forget-password`;
export async function forgetPassword(email) {
    return await post(forgetPasswordEndpoint,  email );
}

const resetPasswordEndpoint = `${baseUrl}/api/Authentication/reset-password`;
export async function resetPassword(data) {
    return await post(resetPasswordEndpoint, data);
}

const checkOtpEndpoint = `${baseUrl}/api/Authentication/check-otp`;
export async function checkOtp(data) {
    return await post(checkOtpEndpoint, data);
}

export async function logout() {
    try {
        const refreshToken = session.getRefreshToken();
        await post(`${baseUrl}/api/Authentication/logout`, { refreshToken });
    } catch (error) {
        console.error('Server logout failed, clearing local session anyway:', error);
    } finally {
        session.clearSession();
        window.location.href = 'Login.html';
    }
}