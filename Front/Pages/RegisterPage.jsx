import { useState } from "react";
import useLogin from "../Hooks/useLogin";

import ModalWindow from "../Components/ModalWindow";

import "../Styles/AuthPageStyle.css";

export default function RegisterPage() {
    const { TryRegister } = useLogin();

    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [displayName, setDisplayName] = useState("");
    const [email, setEmail] = useState("");

    const [visible, setVisible] = useState(false);

    function toggleModal() {
        setVisible(!visible);
    }

    function handleRegister() {
        if (TryRegister(login, password, displayName, email)) {
            toggleModal();
            setLogin("");
            setPassword("");
            setDisplayName("");
            setEmail("");
        }
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

                    <div className="auth-group">
                        <label>Adres e-mail</label>

                        <input
                            type="text"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="Wprowadź nazwę"
                        />
                    </div>

                    <button
                        className="auth-button"
                        type="submit"
                        onClick={handleRegister}
                    >
                        Zarejestruj
                    </button>
                </div>
            </div>
            <ModalWindow
                visible={visible}
                toggleModal={toggleModal}
                message="Wysłano link aktywacyjny na podany adres email. Aby korzystać z serwisu potwierdź swój adres."
                showButtons={false}
            />
        </div>
    );
}
