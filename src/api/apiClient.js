

export const baseUrl = 'https://localhost:7203';

// Shared by every method below so the error-shape handling only
async function handleResponse(response) {
    if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new Error(extractErrorMessage(errorData));
    }

    if (response.status === 204) {
        return null;
    }

    // Check if the response is JSON or plain text
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        return response.json();
    }

    // If it's plain text (like success messages), return text
    return response.text();
}


function extractErrorMessage(errorData) {
    if (!errorData) {
        return 'An error occurred while making the request';
    }

    if (typeof errorData === 'string') {
        return errorData;
    }

    if (typeof errorData === 'object') {
        // Look for your C# middleware's ErrorMessage or Message property first
        return errorData.ErrorMessage || errorData.message || errorData.errorMessage || errorData.title || JSON.stringify(errorData);
    }

    return String(errorData);
}


export async function get(url) {
    const response = await fetch(url);
    return handleResponse(response);
}

export async function post(url, data) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });
    return handleResponse(response);
}

// Added to support authenticated requests (like change-password) cleanly
export async function postWithAuth(url, data, token) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
    });
    return handleResponse(response);
}

export async function put(url, data) {
    const response = await fetch(url, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });
    return handleResponse(response);
}

export async function del(url) {
    const response = await fetch(url, {
        method: 'DELETE'
    });
    return handleResponse(response);
}
