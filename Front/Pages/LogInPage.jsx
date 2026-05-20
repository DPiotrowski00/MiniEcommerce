import { useState } from "react";
import useLogin from "../Hooks/useLogin";

import "../Styles/AuthPageStyle.css";

export default function LogInPage() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");

    const { TryLogin } = useLogin();

    const LogInClick = async () => {
        await TryLogin(login, password);
        window.location.reload();
    };

    return (
        <div className="auth-page">
            <div className="auth-card">
                <h1 className="auth-title">Logowanie</h1>

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

                    <button className="auth-button" onClick={LogInClick}>
                        Zaloguj
                    </button>
                </div>
            </div>
        </div>
    );
}
