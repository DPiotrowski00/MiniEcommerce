import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";

import useOrders from "../Hooks/useOrders";
import useItems from "../Hooks/useItems";

import stringFormatters from "../Helpers/stringFormatters";

import "../Styles/OrderPageStyle.css";

export default function OrderPage() {
    const [order, setOrder] = useState();

    const { id } = useParams();
    const { getOrder } = useOrders();
    const { GetItemById } = useItems();

    const { formatPrice } = stringFormatters();

    const navigate = useNavigate();

    useEffect(() => {
        const fetchOrder = async () => {
            const data = await getOrder(id);
            data.value = 0;

            for (let pos of data.positions) {
                const item = await GetItemById(pos.itemID);
                pos.value = item.price * pos.quantity;
                pos.name = item.name;
                data.value += pos.value;
            }

            setOrder(data);
        };

        fetchOrder();
    }, [id]);

    if (!order) {
        return <div>Ładowanie...</div>;
    }

    return (
        <div className="order-page">
            <div className="order-card">
                <h1 className="order-title">Zamówienie nr {order.id}</h1>

                <table className="order-table">
                    <thead>
                        <tr>
                            <th>Artykuł</th>
                            <th>Ilość</th>
                            <th>Wartość</th>
                        </tr>
                    </thead>

                    <tbody>
                        {order.positions &&
                            order.positions.map((position) => (
                                <tr key={position.itemID}
                                    onClick={() => navigate(`/item/${position.itemID}`)}>
                                    <td>{position.name}</td>
                                    <td>{position.quantity}</td>
                                    <td>{formatPrice(position.value)}</td>
                                </tr>
                            ))}
                    </tbody>
                </table>

                <div className="order-summary">
                    <p>
                        <span>Wartość zamówienia</span>
                        <strong>{formatPrice(order.value)}</strong>
                    </p>

                    <p>
                        <span>Status</span>
                        <strong>{order.status}</strong>
                    </p>
                </div>
            </div>
        </div>
    );
}
