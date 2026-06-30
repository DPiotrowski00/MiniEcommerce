import { useState } from "react";
import useLogin from "../Hooks/useLogin";
import { useTranslation } from "react-i18next";

export default function AccountPage() {
    const [oldPass, setOldPass] = useState("");
    const [newPass, setNewPass] = useState("");

    const { TryChangePassword } = useLogin();

    const confirmPasswordChange = async () => {
        await TryChangePassword(oldPass, newPass);
    };

    return (
        <>
            <div className="password-form">
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
                <button onClick={confirmPasswordChange}>{t("change_pass")}</button>
            </div>
        </>
    );
}
