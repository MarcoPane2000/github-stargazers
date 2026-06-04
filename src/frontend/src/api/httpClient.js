import axios from 'axios';

const httpClient = axios.create({
  // baseURL: 'http://localhost:5020',
  baseURL: 'https://github-stargazers-webapi-e0ggd7apdvh3dkc6.italynorth-01.azurewebsites.net',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const backendMessage = error.response?.data?.detail || 'Errore di connessione al server.';
    return Promise.reject(new Error(backendMessage));
  }
);

export default httpClient;