

export const baseUrl = 'https://localhost:7203';

// Shared by every method below so the error-shape handling only
async function handleResponse(response) {
    if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new Error(extractErrorMessage(errorData));
    }

    // DELETE (and some updates) return 204 No Content - there's no
    // body to parse, so trying response.json() here would throw.
    if (response.status === 204) {
        return null;
    }

    return response.json();
}


function extractErrorMessage(errorData) {
    if (!errorData) {
        return 'An error occurred while making the request';
    }
    if (errorData.errors) {
        return Object.values(errorData.errors).flat().join(' ');
    }
    return errorData.message || 'An error occurred while making the request';
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
