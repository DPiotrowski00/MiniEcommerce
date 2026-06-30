import { useState } from "react";
import { useNavigate } from "react-router-dom";

import useLogin from "../Hooks/useLogin";

import ModalWindow from "../Components/ModalWindow";

export default function PasswordResetPage() {
    const [email, setEmail] = useState("");
    const [modal, setModal] = useState("");

    const navigate = useNavigate();

    const { ResetPassword } = useLogin();

    const handleResetClick = async () => {
        if (await ResetPassword(email)) {
            navigate("/login");
        } else {
            toggleModal();
        }
    };

    function toggleModal() {
        setModal(!modal);
    }

    return (
        <div>
            <input
                type="text"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
            />
            <button onClick={handleResetClick}>{t("reset_password")}</button>
            <ModalWindow
                visible={modal}
                message={t("something_went_wrong")}
                showButtons={false}
                toggleModal={toggleModal}
            />
        </div>
    );
}
