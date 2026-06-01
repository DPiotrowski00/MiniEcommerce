import { apiFetch } from "../API/apiClient";

export default function useOrders() {
    function tryPlaceOrder(items) {
        let passedItems = [];

        for (let i of items) {
            let item = {
                ItemID: i.ItemId,
                Quantity: i.Quantity,
            };
            passedItems = [...passedItems, item];
        }

        const response = apiFetch("/order", {
            method: "PUT",
            body: JSON.stringify({
                Items: passedItems,
            }),
        });

        if (response.ok) {
            return true;
        } else {
            return false;
        }
    }

    return { tryPlaceOrder };
}
