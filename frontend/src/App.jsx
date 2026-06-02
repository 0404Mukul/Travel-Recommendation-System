import {BrowserRouter ,Routes, Route} from 'react-router-dom';
import Dashboard from "./pages/Dashboard";
import Login from './pages/Login';
import ProtectRoute from './components/ProtectRoute';
import Signup from './pages/Signup';


function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path='/' element={<Login/>} />
        <Route path='/signup' element={<Signup/>} />
        <Route path="/dashboard" element={
          <ProtectRoute>
            <Dashboard/>
          </ProtectRoute> 
        }
        />
      </Routes>
    </BrowserRouter>
  )
};

export default App;