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
// Response parsing helper
async function handleResponse(response) {
  if (response.ok) {
    if (response.status === 204) return null;
    return await response.json();
  }

  let errorMessage = `HTTP ${response.status} Error`;

  try {
    const errorData = await response.json();

    if (errorData) {
      const extractedMessages = [];

      // Helper function to extract text strings from any nested error structure
      const extractMessage = (err) => {
        if (!err) return null;
        if (typeof err === 'string') return err;
        if (typeof err === 'object') {
          return err.description || err.errorMessage || err.message || err.title || JSON.stringify(err);
        }
        return String(err);
      };

      // 1. Handle ASP.NET Core Validation Problem Details (errors dictionary)
      if (errorData.errors && typeof errorData.errors === 'object') {
        for (const key in errorData.errors) {
          const fieldErrors = errorData.errors[key];
          if (Array.isArray(fieldErrors)) {
            fieldErrors.forEach((err) => {
              const msg = extractMessage(err);
              if (msg) extractedMessages.push(`${key}: ${msg}`);
            });
          } else {
            const msg = extractMessage(fieldErrors);
            if (msg) extractedMessages.push(`${key}: ${msg}`);
          }
        }
      } 
      // 2. Handle Identity Error Arrays: [{ code: "...", description: "..." }]
      else if (Array.isArray(errorData)) {
        errorData.forEach((err) => {
          const msg = extractMessage(err);
          if (msg) extractedMessages.push(msg);
        });
      } 
      // 3. Handle standard Error Responses: { title: "...", detail: "..." }
      else if (errorData.title || errorData.message || errorData.detail) {
        extractedMessages.push(errorData.detail || errorData.message || errorData.title);
      }

      if (extractedMessages.length > 0) {
        errorMessage = extractedMessages.join(' | ');
      }
    }
  } catch (e) {
    // Response payload was not valid JSON
  }

  throw new Error(errorMessage);
}
function extractErrorMessage(errorData) {
    if (!errorData) return 'An error occurred while making the request';
    if (typeof errorData === 'string') return errorData;

    if (typeof errorData === 'object') {
        // If ASP.NET Core returns standard model validation errors dictionary
        if (errorData.errors && typeof errorData.errors === 'object') {
            const fieldErrors = Object.entries(errorData.errors)
                .map(([field, msgs]) => {
                    const cleanField = field.replace(/^addresses\[\d+\]\./i, '').replace(/^addresses\./i, '');
                    const messageString = Array.isArray(msgs) ? msgs.join(', ') : String(msgs);
                    return `${cleanField}: ${messageString}`;
                })
                .join(' | ');

            if (fieldErrors) return fieldErrors;
        }

        // Fallback checks for common response properties
        return errorData.detail || 
               errorData.ErrorMessage || 
               errorData.message || 
               errorData.errorMessage || 
               errorData.title || 
               JSON.stringify(errorData);
    }

    return String(errorData);
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