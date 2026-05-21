import { apiFetch } from "../API/apiClient";
import getDeviceId from "../Helpers/getDeviceId";

export default function useLogin() {
    const TryRegister = async (Login, Password, DisplayName) => {
        const res = await apiFetch("/login", {
            method: "PUT",
            body: JSON.stringify({
                Login,
                Password,
                DisplayName,
            }),
        });

        if (!res.ok) {
            throw new Error("Register failed");
        }

        return res;
    };

    const TryLogin = async (Login, Password) => {
        const DeviceID = getDeviceId();

        const response = await apiFetch("/login", {
            method: "POST",
            body: JSON.stringify({
                Login,
                Password,
                DeviceID,
            }),
        });

        let accessToken = null;

        if (response.ok) {
            accessToken = await response.text();
        }

        localStorage.setItem("access-token", accessToken);
        accessToken !== null && accessToken !== ""
            ? localStorage.setItem("logInStatus", true)
            : localStorage.setItem("logInStatus", false);
    };

    const TestLogin = async () => {
        await apiFetch("/login", {
            method: "GET",
        });
    };

    const LogOut = async () => {
        await apiFetch("/logout", {
            method: "POST",
        });
    };

    const TryChangePassword = async (OldPass, NewPass) => {
        await apiFetch("/password", {
            method: "POST",
            body: JSON.stringify({
                OldPass,
                NewPass,
            }),
        });
    };

    return { TryRegister, TryLogin, TestLogin, LogOut, TryChangePassword };
}
