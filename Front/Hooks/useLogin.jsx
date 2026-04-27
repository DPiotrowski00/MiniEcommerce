import { apiFetch } from "../API/apiClient";
import getDeviceId from "../Helpers/getDeviceId";

export default function useLogin() {
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

        const accessToken = await response.text();
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

    return { TryLogin, TestLogin, LogOut };
}
