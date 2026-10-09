import { initializeAuth } from '../services/authService.js';

// Top-level await automatically pauses execution of any importing module 
// until authentication is verified!
const isAuthenticated = await initializeAuth();

if (!isAuthenticated) {
    // Stops script execution completely if unauthenticated
    throw new Error('Unauthenticated user redirecting to login...');
}