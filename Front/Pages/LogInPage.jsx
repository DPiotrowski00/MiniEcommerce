import { useState } from "react";
import useLogin from "../Hooks/useLogin";

import ModalWindow from "../Components/ModalWindow";

import "../Styles/AuthPageStyle.css";

export default function LogInPage() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");

    const [message, setMessage] = useState("");
    const [visible, setVisible] = useState(false);

    const { TryLogin } = useLogin();

    function toggleModal() {
        setVisible(!visible);
    }

    const handleLogin = async () => {
        const response = await TryLogin(login, password);
        if (response.ok) {
            window.location.reload();
        } else {
            setMessage(response.message);
            toggleModal();
        }
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

                    <button className="auth-button" onClick={handleLogin}>
                        Zaloguj
                    </button>
                </div>
            </div>
            <ModalWindow
                visible={visible}
                toggleModal={toggleModal}
                message={message}
                showButtons={false}
            />
        </div>
    );
}
