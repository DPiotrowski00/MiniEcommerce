import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import useLogin from "../Hooks/useLogin";

import ModalWindow from "../Components/ModalWindow";

import "../Styles/AuthPageStyle.css";

export default function LogInPage() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");

    const [message, setMessage] = useState("");
    const [visible, setVisible] = useState(false);

    const navigate = useNavigate();

    const { TryLogin } = useLogin();
    const { t } = useTranslation();

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
                <h1 className="auth-title">{t("login")}</h1>

                <div className="auth-form">
                    <div className="auth-group">
                        <label>Login</label>

                        <input
                            type="text"
                            value={login}
                            onChange={(e) => setLogin(e.target.value)}
                            placeholder={t("enter_login_email")}
                        />
                    </div>

                    <div className="auth-group">
                        <label>{t("password")}</label>

                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder={t("enter_password")}
                        />
                    </div>

                    <button className="auth-button" onClick={handleLogin}>
                        {t("log_in")}
                    </button>
                    <p
                        className="auth-link"
                        onClick={() => navigate("/reset-password")}
                    >
                        {t("forgot_password")}
                    </p>
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
