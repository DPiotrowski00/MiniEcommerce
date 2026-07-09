import { apiFetch } from "../API/apiClient";
import getDeviceId from "../Helpers/getDeviceId";

export default function useLogin() {
    const TryRegister = async (Login, Password, DisplayName, Email) => {
        const res = await apiFetch("/login", {
            method: "PUT",
            body: JSON.stringify({
                Login,
                Password,
                DisplayName,
                Email,
            }),
        });

        return res.ok;
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
            const data = await response.json();

            accessToken = data.accessToken;
            localStorage.setItem("csrf-token", data.csrfToken);
        } else {
            const data = await response.json();

            return {
                ok: response.ok,
                message: data.message,
            };
        }

        localStorage.setItem("access-token", accessToken);
        accessToken !== null && accessToken !== ""
            ? localStorage.setItem("logInStatus", true)
            : localStorage.setItem("logInStatus", false);

        return {
            ok: response.ok,
            message: response.message,
        };
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
        const response = await apiFetch("/password", {
            method: "POST",
            body: JSON.stringify({
                OldPass,
                NewPass,
            }),
        });

        return response.ok;
    };

    const ResetPassword = async (email) => {
        const response = await apiFetch("/login/reset-password", {
            method: "POST",
            body: JSON.stringify({
                email,
            }),
        });

        return response.ok;
    };

    return {
        TryRegister,
        TryLogin,
        TestLogin,
        LogOut,
        TryChangePassword,
        ResetPassword,
    };
}
