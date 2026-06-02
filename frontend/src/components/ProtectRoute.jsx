import { Navigate } from "react-router-dom";

function ProtectRoute({ children }) {
    const token = localStorage.getItem("token");

    return token ? children : <Navigate to="/" />;
}

export default ProtectRoute;