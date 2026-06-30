import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useLogin from "../Hooks/useLogin";

import ModalWindow from "../Components/ModalWindow";

import "../Styles/AuthPageStyle.css";

export default function RegisterPage() {
    const { TryRegister } = useLogin();

    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [displayName, setDisplayName] = useState("");
    const [email, setEmail] = useState("");
    const [modalMessage, setModalMessage] = useState("");

    const [visible, setVisible] = useState(false);

    const navigate = useNavigate();

    function toggleModal() {
        setVisible(!visible);
    }

    const handleRegister = async () => {
        const success = await TryRegister(login, password, displayName, email);
        if (success) {
            setModalMessage(t("email_confirmation_message"));
            toggleModal();
            setLogin("");
            setPassword("");
            setDisplayName("");
            setEmail("");
            navigate("/login");
        } else {
            setModalMessage(t("register_attempt_failed"));
            toggleModal();
        }
    };

    return (
        <div className="auth-page">
            <div className="auth-card">
                <h1 className="auth-title">{t("register")}</h1>

                <div className="auth-form">
                    <div className="auth-group">
                        <label>Login</label>

                        <input
                            type="text"
                            value={login}
                            onChange={(e) => setLogin(e.target.value)}
                            placeholder={t("enter_login")}
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

                    <div className="auth-group">
                        <label>{t("display_name")}</label>

                        <input
                            type="text"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            placeholder={t("enter_name")}
                        />
                    </div>

                    <div className="auth-group">
                        <label>{t("email_address")}</label>

                        <input
                            type="text"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder={t("enter_email")}
                        />
                    </div>

                    <button
                        className="auth-button"
                        type="submit"
                        onClick={handleRegister}
                    >
                        {t("register_confirm")}
                    </button>
                </div>
            </div>
            <ModalWindow
                visible={visible}
                toggleModal={toggleModal}
                message={modalMessage}
                showButtons={false}
            />
        </div>
    );
}
