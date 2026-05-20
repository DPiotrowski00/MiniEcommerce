import { Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect } from "react";
import useLogin from "../Hooks/useLogin";

import cart from "../Images/cart.png";

import "../Styles/NavBarStyle.css";

export default function NavBar() {
    const navigate = useNavigate();
    const location = useLocation();

    const logInStatus =
        localStorage.getItem("logInStatus") !== null
            ? localStorage.getItem("logInStatus") === "true"
            : false;

    const { LogOut } = useLogin();

    useEffect(() => {
        if (
            logInStatus === true &&
            (location.pathname === "/login" ||
                location.pathname === "/register")
        ) {
            navigate("/");
        } else {
            if (
                logInStatus === false &&
                location.pathname !== "/" &&
                location.pathname !== "/login" &&
                location.pathname !== "/register"
            ) {
                navigate("/login");
            }
        }
    }, [location]);

    function handleDropdown() {
        document.getElementById("dropdown-content").classList.toggle("show");
    }

    const logOut = async () => {
        await LogOut();
        localStorage.removeItem("logInStatus");
        localStorage.removeItem("access-token");
        window.location.reload();
    };

    return (
        <div className="navbar">
            <div className="navbar-links">
                <Link className="nav-link" to="/">
                    Home
                </Link>

                <Link className="nav-link" to="/login">
                    Logowanie
                </Link>

                <Link className="nav-link" to="/register">
                    Rejestracja
                </Link>
            </div>

            {logInStatus === true && (
                <div className="hidden-menu">
                    <div className="dropdown">
                        <button className="dropbtn" onClick={handleDropdown}>
                            ꜜ
                        </button>

                        <div id="dropdown-content" className="dropdown-content">
                            <Link to="/item/add" onClick={handleDropdown}>
                                Dodaj ofertę
                            </Link>

                            <Link to="/account" onClick={handleDropdown}>
                                Moje konto
                            </Link>
                        </div>
                    </div>

                    <Link to="/" className="cart-link">
                        <img src={cart} alt="koszyk" />
                    </Link>

                    <button className="logout-button" onClick={logOut}>
                        Wyloguj
                    </button>
                </div>
            )}
        </div>
    );
}
