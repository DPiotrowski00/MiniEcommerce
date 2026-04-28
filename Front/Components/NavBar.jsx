import { Link } from "react-router-dom";

import "../Styles/NavBarStyle.css";

export default function NavBar() {
    const logInStatus =
        localStorage.getItem("logInStatus") !== null
            ? localStorage.getItem("logInStatus")
            : false;

    function handleDropdown() {
        document.getElementById("dropdown-content").classList.toggle("show");
    }

    return (
        <div>
            <Link to="/">Home</Link>
            <Link to="/login">Logowanie</Link>
            <Link to="/register">Rejestracja</Link>
            {logInStatus === true && (
                <div className="dropdown">
                    <button className="dropbtn" onClick={handleDropdown}>
                        ꜜ
                    </button>
                    <div id="dropdown-content" className="dropdown-content">
                        <Link to="/item/add" onClick={handleDropdown}>
                            Dodaj ofertę
                        </Link>
                    </div>
                </div>
            )}
        </div>
    );
}
