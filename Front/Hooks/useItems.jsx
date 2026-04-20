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

    const TryCreateItem = async (thumbnail, name, description, price) => {
        const formData = new FormData();
        formData.append("Item.Name", name);
        formData.append("Item.Description", description);
        formData.append("Item.Price", price);
        formData.append("Image", thumbnail);

        const res = await apiFetch(`/items`, {
            method: "PUT",
            body: formData,
        });
        if (res.status === 200) {
            return true;
        } else {
            return false;
        }
    };

    return { GetItems, GetItemById, TryCreateItem };
}
