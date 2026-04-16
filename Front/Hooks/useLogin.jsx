import { apiFetch, setAccessToken } from "../API/apiClient";
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

        const token = await response.text();
        setAccessToken(token);
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
