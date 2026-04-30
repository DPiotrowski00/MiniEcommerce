import { useState } from "react";
import useLogin from "../Hooks/useLogin";

export default function LogInPage() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");

    const { TryLogin, TestLogin } = useLogin();

    const LogInClick = async () => {
        await TryLogin(login, password);
        window.location.reload();
    };

    function Test() {
        TestLogin();
    }

    return (
        <div>
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
            <button onClick={LogInClick}>Zaloguj</button>
            <button onClick={Test}>Testuj</button>
        </div>
    );
}
