import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../services/api";


function Dashboard() {

    const navigate = useNavigate();

    const [prompt, setPrompt] = useState("");
    const [answer, setAnswer] = useState("");

    const askAI = async () => {
    try {

        const response = await API.post("/ai/chat", {
            prompt
        });

        setAnswer(
            JSON.stringify(response.data, null, 2)
        );

    }
    catch(error){

        console.log(error);

        setAnswer(
            JSON.stringify(
                error.response?.data || error.message,
                null,
                2
            )
        );
    }
};
    const logout = () => {

        localStorage.removeItem("token");

        localStorage.removeItem("isLoggedIn");

        navigate("/login");
    };

    return (
        <div>
            <h1>Welcome to Dashboard</h1>
            <h2>Flight AI Assistance</h2>
            <input 
                type="text"
                placeholder="Ask about your flight"
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
            />
            <button onClick={askAI}>
                Ask AI
            </button>
            <pre>{answer}</pre>
            <button onClick={logout}>
                Logout
            </button>
        </div>
    );
}

export default Dashboard;