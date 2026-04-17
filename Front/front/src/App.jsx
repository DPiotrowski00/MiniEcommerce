import { Routes, Route } from "react-router-dom";

import NavBar from "../../Components/NavBar";

import HomePage from "../../Pages/HomePage";
import LogInPage from "../../Pages/LogInPage";
import RegisterPage from "../../Pages/RegisterPage";
import ItemPage from "../../Pages/ItemPage";

export default function App() {
    return (
        <>
            <NavBar />
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/login" element={<LogInPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/item/:id" element={<ItemPage />} />
            </Routes>
        </>
    );
}