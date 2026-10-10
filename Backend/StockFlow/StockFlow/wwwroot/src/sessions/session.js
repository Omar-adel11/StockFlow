// sessions/session.js

export function setSession(result) {
    // Expects result to contain: { name, email, imgUrl, token, role }
    // Note: refreshToken is removed because it is managed via HttpOnly cookies
    if (result.name) sessionStorage.setItem('name', result.name);
    if (result.email) sessionStorage.setItem('email', result.email);
    if (result.imgUrl) sessionStorage.setItem('imgUrl', result.imgUrl);
    if (result.token) sessionStorage.setItem('token', result.token);
    if (result.role) sessionStorage.setItem('role', result.role);
}

export function getAccessToken() {
    return sessionStorage.getItem('token');
}

export function clearSession() {
    sessionStorage.clear();
}