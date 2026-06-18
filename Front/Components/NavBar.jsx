import { Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect, useState, useRef } from "react";
import useLogin from "../Hooks/useLogin";

import cart from "../Images/cart.png";

import "../Styles/NavBarStyle.css";

export default function NavBar() {
    const navigate = useNavigate();
    const location = useLocation();

    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const dropdownRef = useRef(null);

    const logInStatus =
        localStorage.getItem("logInStatus") !== null
            ? localStorage.getItem("logInStatus") === "true"
            : false;

    const { LogOut } = useLogin();

    useEffect(() => {
        if (
            logInStatus === true &&
            (
                location.pathname === "/login" ||
                location.pathname === "/register" ||
                location.pathname === "/reset-password"
            )
        ) {
            navigate("/");
        } else {
            if (
                logInStatus === false &&
                location.pathname !== "/" &&
                location.pathname !== "/login" &&
                location.pathname !== "/register" &&
                location.pathname !== "/reset-password"
            ) {
                navigate("/login");
            }
        }
    }, [location]);

    // zamykanie dropdown po kliknięciu poza nim
    useEffect(() => {
        const handleClickOutside = (event) => {
            if (
                dropdownRef.current &&
                !dropdownRef.current.contains(event.target)
            ) {
                setIsDropdownOpen(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    const handleDropdown = () => {
        setIsDropdownOpen((prev) => !prev);
    };

    const closeDropdown = () => {
        setIsDropdownOpen(false);
    };

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
                    <div className="dropdown" ref={dropdownRef}>
                        <button
                            className="dropbtn"
                            onClick={handleDropdown}
                        >
                            ▼
                        </button>

                        <div
                            className={`dropdown-content ${isDropdownOpen ? "show" : ""}`}
                        >
                            <Link
                                to="/item/add"
                                onClick={closeDropdown}
                            >
                                Dodaj ofertę
                            </Link>

                            <Link
                                to="/account"
                                onClick={closeDropdown}
                            >
                                Moje konto
                            </Link>

                            <Link
                                to="/orders"
                                onClick={closeDropdown}
                            >
                                Moje zamówienia
                            </Link>
                        </div>
                    </div>

                    <Link to="/checkout" className="cart-link">
                        <img src={cart} alt="koszyk" />
                    </Link>

                    <button
                        className="logout-button"
                        onClick={logOut}
                    >
                        Wyloguj
                    </button>
                </div>
            )}
        </div>
    );
}