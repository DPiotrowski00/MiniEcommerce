import { useState } from "react";

export default function LogInPage() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");

    function VerifyPassword() {
        setLogin("");
        setPassword("");
    }

    return (
        <div>
            <input type="text" value={login} onChange={(e) => setLogin(e.target.value)} />
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            <button onClick={VerifyPassword}>Zaloguj</button>
        </div>
    );
}
