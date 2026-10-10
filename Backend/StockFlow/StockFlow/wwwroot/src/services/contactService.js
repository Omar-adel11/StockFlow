import { post, baseUrl  } from '../api/apiClient.js';

const contactEndpoint =  `${baseUrl}/api/Contact`;
export async function sendContactMessage(data) {
    return await post(contactEndpoint,data);
}