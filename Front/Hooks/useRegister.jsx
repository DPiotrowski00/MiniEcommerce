import { apiFetch } from "../API/apiClient";

export default function useRegister() {
    const tryRegister = async (Login, Password, DisplayName) => {
        const res = await apiFetch("/login", {
            method: "PUT",
            body: JSON.stringify({
                Login,
                Password,
                DisplayName
            })
        });

        if (!res.ok) {
            throw new Error("Register failed");
        }

        return res;
    }

    return { tryRegister };
}