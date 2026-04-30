import { useState } from "react";
import useLogin from "../Hooks/useLogin";

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
                    Old password
                    <input
                        type="password"
                        value={oldPass}
                        onChange={(e) => setOldPass(e.target.value)}
                    />
                </label>
                <label>
                    New password
                    <input
                        type="password"
                        value={newPass}
                        onChange={(e) => setNewPass(e.target.value)}
                    />
                </label>
                <button onClick={confirmPasswordChange}>Change password</button>
            </div>
        </>
    );
}
