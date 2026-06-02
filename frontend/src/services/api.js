import axios from "axios"

const API =axios.create({
    baseURL: "http://localhost:5179/api"
});

export default API;