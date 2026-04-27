import { Link } from "react-router-dom";
import { useState } from "react";

export default function NavBar() {
    const [dropDownVisible, setDropDownVisible] = useState(false);

    const logInStatus =
        localStorage.getItem("logInStatus") !== null
            ? localStorage.getItem("logInStatus")
            : false;

    function toggleDropDown() {
        dropDownVisible === false
            ? setDropDownVisible(true)
            : setDropDownVisible(false);

        console.log(dropDownVisible);
    }

    return (
        <div>
            <Link to="/">Home</Link>
            <Link to="/login">Logowanie</Link>
            <Link to="/register">Rejestracja</Link>
            {logInStatus && <Link to="/item/add">Dodaj artykuł</Link>}
            <button onClick={toggleDropDown}>ꜜ</button>
            {dropDownVisible && <label>dropdown</label>}
        </div>
    );
}
