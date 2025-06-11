import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api', // change to your actual backend URL
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;
