import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import useOrders from "../Hooks/useOrders";
import useItems from "../Hooks/useItems";
import { useTranslation } from "react-i18next";

import stringFormatters from "../Helpers/stringFormatters";

import "../Styles/MyOrdersPageStyle.css";

export default function MyOrdersPage() {
    const [orders, setOrders] = useState([]);

    const { getMyOrders } = useOrders();
    const { GetItemById } = useItems();
    const { formatPrice, formatDate } = stringFormatters();
    const navigate = useNavigate();

    const { t } = useTranslation();

    useEffect(() => {
        const fetchOrders = async () => {
            const data = await getMyOrders();

            for (let ord of data) {
                ord.value = 0;
                for (let pos of ord.positions) {
                    const item = await GetItemById(pos.itemID);
                    pos.value = item.price * pos.quantity;
                    pos.name = item.name;
                    ord.value += pos.value;
                }
            }

            setOrders(data);
        };

        fetchOrders();
    }, []);
    if (!orders) {
        return <p>{t("loading")}</p>
    }
    if (orders.length === 0) {
        return <p>{t("you_dont_have_orders")}</p>;
    }

    return (
        <div className="my-orders-page">
            <div className="my-orders-card">
                <h1 className="my-orders-title">{t("my_orders")}</h1>

                <table className="my-orders-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>{t("order_date")}</th>
                            <th>Status</th>
                            <th>{t("value")}</th>
                        </tr>
                    </thead>

                    <tbody>
                        {orders.map((order) => (
                            <tr
                                key={order.id}
                                onClick={() => navigate(`/order/${order.id}`)}
                            >
                                <td>{order.id}</td>
                                <td>{formatDate(order.timeStamp)}</td>
                                <td>{order.status}</td>
                                <td>{formatPrice(order.value)}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
