// services/authService.js
import { post, baseUrl, postWithAuth, getReadableErrorMessage } from "../api/apiClient.js";
import * as session from '../sessions/session.js';

const loginEndpoint = `${baseUrl}/api/Authentication/login`;
export async function login(data) {
    const result = await post(loginEndpoint, data);
    session.setSession(result);
    return result;
}

const registerEndpoint = `${baseUrl}/api/Authentication/signup`;
export async function register(formData) {
    const response = await fetch(registerEndpoint, {
        method: 'POST',
        body: formData,
        credentials: 'include' // CRITICAL: Receives Set-Cookie header on signup
    });
    
    if (!response.ok) {
        let errorData = null;
        try {
            const text = await response.text();
            if (text.trim()) {
                try {
                    errorData = JSON.parse(text);
                } catch {
                    errorData = text;
                }
            }
        } catch {
            // Friendly status fallback
        }

        throw new Error(getReadableErrorMessage(errorData, response.status));
    }

    if (response.status === 204) return null;
    const result = await response.json();
    session.setSession(result);
    return result;
}

const changePasswordEndpoint = `${baseUrl}/api/Authentication/change-password`;
export async function changePassword(data) {
    return await postWithAuth(changePasswordEndpoint, data);
}

const forgetPasswordEndpoint = `${baseUrl}/api/Authentication/forget-password`;
export async function forgetPassword(email) {
    return await post(forgetPasswordEndpoint, email);
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
        // Backend reads cookie, revokes refresh token, and deletes cookie via Response.Cookies.Delete
        await post(`${baseUrl}/api/Authentication/logout`, {});
    } catch (error) {
        console.error('Server logout failed, clearing local session anyway:', error);
    } finally {
        session.clearSession();
        window.location.href = 'Login.html';
    }
}

export async function initializeAuth() {
    const token = session.getAccessToken();

    // 1. If access token is already in sessionStorage, session is valid
    if (token) {
        return true;
    }

    // 2. Attempt silent refresh via HttpOnly cookie
    try {
        const response = await fetch(`${baseUrl}/api/Authentication/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include' // Transmits HttpOnly cookie
        });

        if (response.ok) {
            const result = await response.json();
            session.setSession(result); // Restore token & user session
            return true;
        }
    } catch (error) {
        console.error('Silent session restoration failed:', error);
    }

    // 3. ONLY execute cleanup & redirect IF the refresh fetch failed
    session.clearSession();
    window.location.href = 'Login.html';
    return false;
}