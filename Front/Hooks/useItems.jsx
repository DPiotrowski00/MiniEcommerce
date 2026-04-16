import { apiFetch } from "../API/apiClient";

export default function useItems() {
    const GetItems = async () => {
        const res = await apiFetch("/items", {
            method: "GET",
        });
        return res.json();
    };

    return { GetItems };
}
