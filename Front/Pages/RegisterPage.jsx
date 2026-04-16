import { useState } from "react";
import useRegister from "../Hooks/useRegister"

export default function RegisterPage() {
    const { tryRegister } = useRegister();

    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [displayName, setDisplayName] = useState("");
    const [message, setMessage] = useState("msg");

    function RegisterClick() {
        tryRegister(login, password, displayName);
    }

    return (
        <div className="registerDiv">
            <input type="text" value={login} onChange={(e) => setLogin(e.target.value)}/>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)}/>
            <input type="text" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
            <button type="submit" onClick={RegisterClick}>Register</button>
            <label>{message}</label>
        </div>
    );
}