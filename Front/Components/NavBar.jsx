import { Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect, useState, useRef } from "react";
import { useTranslation } from "react-i18next"
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
    const { t, i18n } = useTranslation();

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
                    {t("home")}
                </Link>

                <Link className="nav-link" to="/login">
                    {t("login")}
                </Link>

                <Link className="nav-link" to="/register">
                    {t("register")}
                </Link>
            </div>

            {logInStatus === true && (
                <>
                    <div className="language-switcher">
                            <button
                                className={`lang-btn ${i18n.language === "pl" ? "active" : ""
                                    }`}
                                onClick={() => i18n.changeLanguage("pl")}
                                aria-label="Polski"
                            >
                                <img
                                    src="/public/flags/pl.svg"
                                    alt="Polski"
                                />
                            </button>

                            <button
                                className={`lang-btn ${i18n.language === "en" ? "active" : ""
                                    }`}
                                onClick={() => i18n.changeLanguage("en")}
                                aria-label="English"
                            >
                                <img
                                    src="public/flags/gb.svg"
                                    alt="English"
                                />
                            </button>
                        </div>
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
                                    {t("add_offer")}
                                </Link>

                                <Link
                                    to="/account"
                                    onClick={closeDropdown}
                                >
                                    {t("my_account")}
                                </Link>

                                <Link
                                    to="/orders"
                                    onClick={closeDropdown}
                                >
                                    {t("my_orders")}
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
                            {t("log_out")}
                        </button>
                    </div>
                </>
            )}
        </div>
    );
}