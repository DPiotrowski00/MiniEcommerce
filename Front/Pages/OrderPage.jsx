import { useSearchParams } from "react-router-dom";
import { useState, useEffect } from "react";

import useOrders from "../Hooks/useOrders";

import formatPrice from "../Helpers/stringFormatters";

export default function OrderPage() {
    const [order, setOrder] = useState();

    const [searchParams] = useSearchParams();
    const id = searchParams.get("id");

    const { getOrder } = useOrders();

    useEffect(() => {
        const fetchOrder = async () => {
            const data = await getOrder(id);
            setOrder(data);
        };

        fetchOrder();
    }, [id]);

    if (!order) {
        return <div>Ładowanie...</div>;
    }

    return (
        <div>
            <h1>Zamówienie nr {order.ID}</h1>
            <table>
                <thead>
                    <tr>
                        <th>Artykuł</th>
                        <th>Ilość</th>
                        <th>Wartość</th>
                    </tr>
                </thead>
                <tbody>
                    {order.Positions &&
                        order.Positions.map((position) => {
                            <tr>
                                <td>{position.ArticleID}</td>
                                <td>{position.Quantity}</td>
                                <td>{formatPrice(position.Value)}</td>
                            </tr>;
                        })}
                </tbody>
            </table>
            <p>Wartość zamówienia: {formatPrice(order.Value)}</p>
            <p>Status zamówienia: {order.Status}</p>
        </div>
    );
}
