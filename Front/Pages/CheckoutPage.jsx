import { useState, useEffect } from "react";
import { tryPlaceOrder } from "../Hooks/useOrders";

export default function CheckoutPage() {
    const [items, setItems] = useState([]);

    useEffect(() => {
        setItems(localStorage.getItem("cart"));
    });

    return (
        <div>
            <table>
                <tr>
                    <th>Artykuł</th>
                    <th>Wartość</th>
                </tr>
                {items.map((item) => {
                    <tr>
                        <td>{item.ID}</td>
                        <td>{item.Value}</td>
                    </tr>;
            })}
            </table>
        </div>
    );
}
