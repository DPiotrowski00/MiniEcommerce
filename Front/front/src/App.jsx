import { Routes, Route } from "react-router-dom";
import HomePage from "../../Pages/HomePage";
import NavBar from "../../Components/NavBar";
import LogInPage from "../../Pages/LogInPage";

export default function App() {
    return (
        <>
            <NavBar />
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/login" element={<LogInPage />} />
            </Routes>
        </>
    );
}