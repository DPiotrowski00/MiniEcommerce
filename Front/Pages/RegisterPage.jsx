import { useState } from "react";
import useLogin from "../Hooks/useLogin";

import "../Styles/AuthPageStyle.css";

export default function RegisterPage() {
    const { TryRegister } = useLogin();

    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [displayName, setDisplayName] = useState("");

    function RegisterClick() {
        TryRegister(login, password, displayName);
    }

    return (
        <div className="auth-page">
            <div className="auth-card">
                <h1 className="auth-title">Rejestracja</h1>

                <div className="auth-form">
                    <div className="auth-group">
                        <label>Login</label>

                        <input
                            type="text"
                            value={login}
                            onChange={(e) => setLogin(e.target.value)}
                            placeholder="Wprowadź login"
                        />
                    </div>

                    <div className="auth-group">
                        <label>Hasło</label>

                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="Wprowadź hasło"
                        />
                    </div>

                    <div className="auth-group">
                        <label>Nazwa wyświetlana</label>

                        <input
                            type="text"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            placeholder="Wprowadź nazwę"
                        />
                    </div>

                    <button
                        className="auth-button"
                        type="submit"
                        onClick={RegisterClick}
                    >
                        Zarejestruj
                    </button>
                </div>
            </div>
        </div>
    );
}
