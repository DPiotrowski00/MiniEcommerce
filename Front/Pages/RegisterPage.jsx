import { useState } from "react";
import useLogin from "../Hooks/useLogin";

export default function RegisterPage() {
    const { TryRegister } = useLogin();

    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [displayName, setDisplayName] = useState("");

    function RegisterClick() {
        TryRegister(login, password, displayName);
    }

    return (
        <div className="registerDiv">
            <input
                type="text"
                value={login}
                onChange={(e) => setLogin(e.target.value)}
            />
            <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
            />
            <input
                type="text"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
            />
            <button type="submit" onClick={RegisterClick}>
                Register
            </button>
        </div>
    );
}
