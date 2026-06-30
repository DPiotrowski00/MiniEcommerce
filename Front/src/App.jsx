import { Routes, Route } from "react-router-dom";

import NavBar from "../Components/NavBar";

import HomePage from "../Pages/HomePage";
import LogInPage from "../Pages/LogInPage";
import RegisterPage from "../Pages/RegisterPage";
import ItemPage from "../Pages/ItemPage";
import ItemCreationPage from "../Pages/ItemCreationPage";
import AccountPage from "../Pages/AccountPage";
import CheckoutPage from "../Pages/CheckoutPage";
import OrderPage from "../Pages/OrderPage";
import MyOrdersPage from "../Pages/MyOrdersPage";
import PasswordResetPage from "../Pages/PasswordResetPage";

import "../src/i18n";

export default function App() {
    console.log("ENV", import.meta.env);
    console.log("API", import.meta.env.VITE_API_ADDRESS);
    return (
        <>
            <NavBar />
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/login" element={<LogInPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/item/:id" element={<ItemPage />} />
                <Route path="/account" element={<AccountPage />} />
                <Route path="/item/add" element={<ItemCreationPage />} />
                <Route path="/checkout" element={<CheckoutPage />} />
                <Route path="/order/:id" element={<OrderPage />} />
                <Route path="/orders" element={<MyOrdersPage />} />
                <Route path="/reset-password" element={<PasswordResetPage />} />
            </Routes>
        </>
    );
}