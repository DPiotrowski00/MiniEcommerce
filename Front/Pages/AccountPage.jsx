import { useState } from "react";
import useLogin from "../Hooks/useLogin";
import { useTranslation } from "react-i18next";
import ModalWindow from "../Components/ModalWindow";
import "../Styles/AccountPageStyle.css";

export default function AccountPage() {
    const [oldPass, setOldPass] = useState("");
    const [newPass, setNewPass] = useState("");
    const [repeatNewPass, setRepeatNewPass] = useState("");
    const [modalMessage, setModalMessage] = useState("");
    const [modalVisible, setModalVisible] = useState(false);

    const { TryChangePassword } = useLogin();

    const { t } = useTranslation();

    function toggleModal() {
        setModalVisible(!modalVisible);
    }

    const confirmPasswordChange = async () => {
        if (newPass !== repeatNewPass) {
            setModalMessage(t("passwords_do_not_match"));
            toggleModal();
        } else {
            if (newPass === oldPass) {
                setModalMessage(t("old_new_passwords_cant_be_the_same"));
                toggleModal();
            } else {
                if (await TryChangePassword(oldPass, newPass)) {
                    setModalMessage(t("password_change_succeeded"));
                    toggleModal();
                } else {
                    setModalMessage(t("password_change_failed"));
                    toggleModal();
                }
            }
        }
    };

    return (
        <div className="account-page">
            <div className="password-form">
                <h2 className="password-title">
                    {t("change_pass")}
                </h2>

                <label>
                    {t("old_pass")}
                    <input
                        type="password"
                        value={oldPass}
                        onChange={(e) => setOldPass(e.target.value)}
                    />
                </label>

                <label>
                    {t("new_pass")}
                    <input
                        type="password"
                        value={newPass}
                        onChange={(e) => setNewPass(e.target.value)}
                    />
                </label>

                <label>
                    {t("repeat_password")}
                    <input
                        type="password"
                        value={repeatNewPass}
                        onChange={(e) => setRepeatNewPass(e.target.value)}
                    />
                </label>

                <button onClick={confirmPasswordChange}>
                    {t("change_pass")}
                </button>
            </div>
            <ModalWindow visible={modalVisible} message={modalMessage} showButtons={false} toggleModal={toggleModal}/>
        </div>
    );
}
