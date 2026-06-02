import {useState, useEffect} from 'react';
import {Link} from 'react-router-dom';
import Navbar from "../components/Navbar";
import InputField from "../components/InputFields";
import Button from "../components/Buttons";
import { useNavigate } from 'react-router-dom';
import '../style/Auth.css';
import API from "../services/api";


function Login(){
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const isLoggedIn = localStorage.getItem("isLoggedIn");
        if (isLoggedIn === "true") {
            navigate("/dashboard");
        }
    }, [navigate]);

    const handleLogin = async (e) => {
        e.preventDefault();

        if (email === "" || password === "") {
            setError("Please fill all fields");
            return;
        }

        setError("");
        setLoading(true);

        try {
            const response = await API.post("/auth/login", {
                email: email,
                password: password
            });
            localStorage.setItem("token",response.data.token);

            console.log(response.data);
            alert(response.data.message);

            localStorage.setItem("isLoggedIn", "true");
            navigate("/dashboard");
        }
        catch(error) {
            console.log(error);
            setError("Login failed");
        }
        finally {
            setLoading(false);
        }
    };


    return(
        <>
        <Navbar/>
        <div className='auth-container'>
            <form className='auth-form' onSubmit={handleLogin}>
                <h2>Login</h2>
                {
                    error && (
                        <p className="error">{error}</p>
                    )
                }

                <InputField
                    type='email'
                    placeholder='Enter Email'
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                />

                <InputField
                    type='password'
                    placeholder='Enter Password'
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />


                <Button 
                    type='submit' 
                    text={loading? "Loading...": "Login"}
                />

                <p> Don't have an account?
                    <Link to='/signup'> Signup</Link>
                </p>
            </form>
        </div>
        </>
    );
}

export default Login;
