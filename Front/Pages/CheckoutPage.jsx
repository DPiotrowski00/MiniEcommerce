import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import useOrders from "../Hooks/useOrders";
import stringFormatters from "../Helpers/stringFormatters";

export default function CheckoutPage() {
    const [items, setItems] = useState([]);
    const { tryPlaceOrder } = useOrders();
    const { formatPrice } = stringFormatters();

    const navigate = useNavigate();

    useEffect(() => {
        setItems(JSON.parse(localStorage.getItem("cart")));
    }, []);

    function handleQuantityChange(item, modifier) {
        const updatedItems = items
            .map((i) => {
                if (i.ItemId === item.ItemId) {
                    return {
                        ...i,
                        Quantity: i.Quantity + modifier,
                    };
                }

                return i;
            })
            .filter((i) => i.Quantity > 0);

        setItems(updatedItems);

        localStorage.setItem("cart", JSON.stringify(updatedItems));
    }

    return (
        <div>
            <h1>CHECKOUT</h1>

            <table>
                <thead>
                    <tr>
                        <th>Zdjęcie</th>
                        <th>Nazwa</th>
                        <th>Cena jednostkowa</th>
                        <th>Ilość</th>
                        <th>Cena całościowa</th>
                    </tr>
                </thead>
                <tbody>
                    {items &&
                        items.map((item) => (
                            <tr
                                key={item.ItemId}
                                onClick={() => navigate(`/item/${item.ItemId}`)}
                            >
                                <td>
                                    <img
                                        src={`https://localhost:7153${item.PicURL}`}
                                        width="50"
                                    />
                                </td>
                                <td>{item.Name}</td>
                                <td>{formatPrice(item.Price)}</td>
                                <td>
                                    <div>
                                        <button
                                            onClick={() =>
                                                handleQuantityChange(item, -1)
                                            }
                                        >
                                            -
                                        </button>
                                        <span>
                                            {formatPrice(item.Quantity)}
                                        </span>
                                        <button
                                            onClick={() =>
                                                handleQuantityChange(item, 1)
                                            }
                                        >
                                            +
                                        </button>
                                    </div>
                                </td>
                                <td>{item.Quantity}</td>
                                <td>
                                    {formatPrice(item.Quantity * item.Price)}
                                </td>
                            </tr>
                        ))}
                </tbody>
            </table>
        </div>
    );
}
