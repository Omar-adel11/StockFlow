// apiClient.js
import { getAccessToken, getRefreshToken, setSession, clearSession } from '../sessions/session.js';

export const baseUrl = 'https://localhost:7203';

// 1. Separate handler for token refresh to avoid infinite loops
async function refreshAccessToken() {
    const refreshToken = getRefreshToken();
    const accessToken = getAccessToken();

    if (!refreshToken) {
        throw new Error('No refresh token available');
    }

    // Call your backend endpoint responsible for refreshing tokens
    const response = await fetch(`${baseUrl}/api/Auth/refresh-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token: accessToken, refreshToken: refreshToken })
    });

    if (!response.ok) {
        // Refresh token is invalid or expired -> force logout
        clearSession();
        window.location.href = 'Login.html';
        throw new Error('Session expired. Please log in again.');
    }

    const data = await response.json();
    // Update local storage/session storage with the new tokens
    setSession(data);
    return data.token; // Return new access token
}

// 2. Central request wrapper with automatic 401 retry
async function fetchWithAuth(url, options = {}) {
    let token = getAccessToken();

    // Attach Authorization header if token exists
    options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`
    };

    let response = await fetch(url, options);

    // 3. If unauthorized (401), attempt to refresh token and retry ONCE
    if (response.status === 401) {
        try {
            const newToken = await refreshAccessToken();

            // Retry original request with the new access token
            options.headers['Authorization'] = `Bearer ${newToken}`;
            response = await fetch(url, options);
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
    // Backend validation shape: { field: "request", errors: ["..."] }
    if (value.field && value.errors != null) {
      const message = valueToMessage(value.errors);
      return message ? `${value.field}: ${message}` : String(value.field);
    }

    // ASP.NET validation shape: { errors: { Field: ["..."] } }
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

    // Prefer explicit human-readable backend messages.
    for (const key of ['detail', 'message', 'errorMessage', 'ErrorMessage', 'description', 'title', 'error']) {
      if (value[key] != null) {
        const message = valueToMessage(value[key]);
        if (message) return message;
      }
    }

    // Some middleware returns an object keyed by indexes (0, 1, ...).
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
    // Keep the friendly status fallback below.
  }

  throw new Error(getReadableErrorMessage(errorData, response.status));
}
// --- Exported HTTP Methods ---

export async function get(url) {
    const response = await fetch(url);
    return handleResponse(response);
}

export async function post(url, data) {
    const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
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