import { useState, useEffect } from "react";
import useOrders from "../Hooks/useOrders";

export default function CheckoutPage() {
    const [orders, setOrders] = useState([]);
    const { getOrders } = useOrders();

    useEffect(() => {
        setOrders(getOrders());
    });

    return (
        <>
            <table>
                <tr>
                    <th>Order number</th>
                    <th>Value</th>
                </tr>
                {orders.map((order) => {
                    <tr>
                        <table>
                            <tr>
                                <th>Item name</th>
                                <th>Quantity</th>
                                <th>Value</th>
                            </tr>
                        </table>
                    </tr>
            })}
            </table>
        </>
    );
}
