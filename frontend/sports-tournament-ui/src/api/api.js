import axios from "axios";

const api = axios.create({
  baseURL: "http://localhost:5072/api",
});

export default api;
