import {Link} from 'react-router-dom'
import { useNavigate} from 'react-router-dom';

function Navbar({children}) { 
    const navigate = useNavigate();

    const handleLogout = () =>{
        localStorage.removeItem("isLoggedIn")
        navigate("/");
    };

    return (
        <>
            <nav className='navbar'>
                <h2>Welcome to Mukul's Airline </h2>

                <div>
                    <Link to="/">Login</Link>
                    <Link to="/signup">Signup</Link>
                    <Link to="/dashboard">Dashboard</Link>
                </div>

                <button onClick={handleLogout}>
                    Logout
                </button>
            </nav>
            {children}
        </>
    );
}

export default Navbar;