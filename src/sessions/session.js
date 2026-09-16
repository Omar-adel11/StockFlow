export function setSession(result) {
    // Expects result to contain: { name, email, imgUrl, token, refreshToken }
    sessionStorage.setItem('name', result.name || '');
    sessionStorage.setItem('email', result.email || '');
    sessionStorage.setItem('imgUrl', result.imgUrl || '');
    sessionStorage.setItem('token', result.token || '');
    sessionStorage.setItem('refreshToken', result.refreshToken || '');
}

export function getAccessToken() {
    return sessionStorage.getItem('token');
}

export function getRefreshToken() {
    return sessionStorage.getItem('refreshToken');
}

export function clearSession() {
    sessionStorage.clear();
}