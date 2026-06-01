import { apiFetch } from "../API/apiClient";

export default function useOrders() {
    async function tryPlaceOrder(items) {
        let passedItems = [];

        for (let i of items) {
            let item = {
                ItemID: i.ItemId,
                Quantity: i.Quantity,
            };
            passedItems = [...passedItems, item];
        }

        const response = await apiFetch("/order", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                Items: passedItems,
            }),
        });

        return response.ok;
    }

    return { tryPlaceOrder };
}
