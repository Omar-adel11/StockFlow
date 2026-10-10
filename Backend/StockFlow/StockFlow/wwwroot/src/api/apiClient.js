// api/apiClient.js
import { getAccessToken, setSession, clearSession } from '../sessions/session.js';

export const baseUrl = '';

// Helper options to ensure cookies are included in every fetch request
const fetchOptions = (options = {}) => ({
    ...options,
    credentials: 'include' // CRITICAL: Allows browser to send & receive HttpOnly cookies
});

// 1. Refresh token handler (no longer requires passing token in payload)
async function refreshAccessToken() {
    // Call the updated controller endpoint: POST /api/Authentication/refresh
    const response = await fetch(`${baseUrl}/api/Authentication/refresh`, fetchOptions({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    }));

    if (!response.ok) {
        // Cookie is invalid or expired -> clear local state and force login
        clearSession();
        window.location.href = 'Login.html';
        throw new Error('Session expired. Please log in again.');
    }

    const data = await response.json();
    
    // Update stored user details and in-memory/session access token
    setSession(data);
    return data.token; // Returns new access token
}

// 2. Central request wrapper with automatic 401 retry
async function fetchWithAuth(url, options = {}) {
    let token = getAccessToken();

    options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`
    };

    let response = await fetch(url, fetchOptions(options));

    // 3. If unauthorized (401), attempt to refresh token via cookie and retry ONCE
    if (response.status === 401) {
        try {
            const newToken = await refreshAccessToken();

            // Retry original request with the new access token
            options.headers['Authorization'] = `Bearer ${newToken}`;
            response = await fetch(url, fetchOptions(options));
        } catch (error) {
            clearSession();
            window.location.href = 'Login.html';
            throw error;
        }
    }

    return handleResponse(response);
}

// Response parsing helper
function valueToMessage(value) {
    if (value == null) return '';

    if (typeof value === 'string') return value.trim();

    if (typeof value === 'number' || typeof value === 'boolean') {
        return String(value);
    }

    if (Array.isArray(value)) {
        return value
            .flatMap(item => {
                const message = valueToMessage(item);
                return message ? [message] : [];
            })
            .filter(Boolean)
            .join(' | ');
    }

    if (typeof value === 'object') {
        if (value.field && value.errors != null) {
            const message = valueToMessage(value.errors);
            return message ? `${value.field}: ${message}` : String(value.field);
        }

        if (value.errors != null) {
            if (Array.isArray(value.errors)) {
                return valueToMessage(value.errors);
            }

            if (typeof value.errors === 'object') {
                return Object.entries(value.errors)
                    .map(([field, messages]) => {
                        const message = valueToMessage(messages);
                        return message ? `${field}: ${message}` : '';
                    })
                    .filter(Boolean)
                    .join(' | ');
            }
        }

        for (const key of ['detail', 'message', 'errorMessage', 'ErrorMessage', 'description', 'title', 'error']) {
            if (value[key] != null) {
                const message = valueToMessage(value[key]);
                if (message) return message;
            }
        }

        const nested = Object.entries(value)
            .filter(([key]) => !['status', 'statusCode', 'traceId', 'type', 'instance'].includes(key))
            .map(([, item]) => valueToMessage(item))
            .filter(Boolean);

        return nested.join(' | ');
    }

    return String(value);
}

function fallbackStatusMessage(status) {
    switch (status) {
        case 400: return 'The request could not be processed. Please check the entered data.';
        case 401: return 'Your session has expired or you are not authenticated. Please log in again.';
        case 403: return 'You do not have permission to perform this action.';
        case 404: return 'The requested resource was not found.';
        case 409: return 'This operation conflicts with the current data.';
        case 422: return 'The submitted data is invalid. Please check the entered values.';
        case 500: return 'A server error occurred. Please try again later.';
        default: return 'The request could not be completed. Please try again.';
    }
}

export function getReadableErrorMessage(errorData, status = 0) {
    const message = valueToMessage(errorData);
    return message || fallbackStatusMessage(status);
}

async function handleResponse(response) {
    if (response.ok) {
        if (response.status === 204) return null;

        const text = await response.text();
        if (!text.trim()) return null;

        try {
            return JSON.parse(text);
        } catch {
            return text;
        }
    }

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
        // Keep fallback status
    }

    throw new Error(getReadableErrorMessage(errorData, response.status));
}

// --- Exported HTTP Methods ---

export async function get(url) {
    const response = await fetch(url, fetchOptions());
    return handleResponse(response);
}

export async function post(url, data) {
    const response = await fetch(url, fetchOptions({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    }));
    return handleResponse(response);
}

// Authenticated helper methods using fetchWithAuth
export async function getWithAuth(url) {
    return fetchWithAuth(url, { method: 'GET' });
}

export async function postWithAuth(url, data) {
    return fetchWithAuth(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
}

export async function putWithAuth(url, data) {
    return fetchWithAuth(url, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
}

export async function delWithAuth(url) {
    return fetchWithAuth(url, { method: 'DELETE' });
}