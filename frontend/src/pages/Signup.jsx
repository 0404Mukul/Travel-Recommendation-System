import {useState} from 'react';
import {Link} from 'react-router-dom';
import Navbar from "../components/Navbar";
import InputField from "../components/InputFields";
import '../style/Auth.css';
import API from "../services/api"

function Signup(){
    const [name,setName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');       

    const handleSignup = async (e) => {
    e.preventDefault();

    if(name === "" || email === "" || password === ""){
        alert("Please fill all fields");
        return;
    }

    try{
        const response = await API.post("/auth/signup", {
            name: name,
            email: email,
            password: password
        });

        alert(response.data.message);

        setName("");
        setEmail("");
        setPassword("");
    }
    catch(error){
        console.log(error);
        alert("Signup Failed");
    }
}

    return(
        <Navbar>
            <div className='auth-container'>
                <form className='auth-form' onSubmit={handleSignup}>
                    <h2>Signup</h2>
                    <InputField  
                        type='text'
                        placeholder='Enter Name'
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
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
                    <button type='submit'>Signup</button>

                    <p> Already have an account?
                        <Link to='/'> Login</Link>
                    </p>
                </form>
            </div>   
        </Navbar> 
    );
}

export default Signup;