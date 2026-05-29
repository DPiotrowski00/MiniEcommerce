import { apiFetch } from "../API/apiClient";

export default function useOrders() {
    function tryPlaceOrder(items) {
        const response = apiFetch("/order", {
            method: "POST",
            body: items,
        });

        if (response.ok) {
            return true;
        } else {
            return false;
        }
    }

    return { tryPlaceOrder };
}
