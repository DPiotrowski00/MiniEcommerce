import { useState, useEffect } from "react";
import useOrders from "../Hooks/useOrders";

export default function CheckoutPage() {
    const [items, setItems] = useState([]);
    const { tryPlaceOrder } = useOrders();

    useEffect(() => {
        setItems(localStorage.getItem("cart"));
    });

    return (
        <div>
            <h1>CHECKOUT</h1>

            <table>
                <thead>
                    <tr>
                        <th>Artykuł</th>
                        <th>Wartość</th>
                    </tr>
                </thead>

                <tbody>
                    {items &&
                        items.map((item) => (
                            <tr key={item.ID}>
                                <td>{item.ID}</td>
                                <td>{item.Value}</td>
                            </tr>
                        ))}
                </tbody>
            </table>
        </div>
    );
}
