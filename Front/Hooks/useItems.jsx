import { apiFetch } from "../API/apiClient";

export default function useItems() {
    const GetItems = async () => {
        const res = await apiFetch("/items", {
            method: "GET",
        });
        return res.json();
    };

    const GetItemById = async (id) => {
        const res = await apiFetch(`/items/${id}`, {
            method: "GET",
        });
        return res.json();
    };

    return { GetItems, GetItemById };
}
