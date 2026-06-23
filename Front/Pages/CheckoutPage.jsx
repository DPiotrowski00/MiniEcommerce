import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import useOrders from "../Hooks/useOrders";
import useItems from "../Hooks/useItems";

import stringFormatters from "../Helpers/stringFormatters";

import ModalWindow from "../Components/ModalWindow";

import "../Styles/CheckoutPageStyle.css";

export default function CheckoutPage() {
    const API_URL = import.meta.env.VITE_API_ADDRESS;

    const [items, setItems] = useState([]);
    const [modalVisible, setModalVisible] = useState(false);
    const [message, setMessage] = useState("");

    const { tryPlaceOrder } = useOrders();
    const { GetItemById } = useItems();
    const { formatPrice } = stringFormatters();
    const navigate = useNavigate();

    function toggleModal() {
        setModalVisible(!modalVisible);
    }

    useEffect(() => {
        const func = async () => {
            const loadedItems = localStorage.getItem("cart")
                ? JSON.parse(localStorage.getItem("cart"))
                : [];

            for (let index = 0; index < loadedItems.length; index++) {
                const fullItem = await GetItemById(loadedItems[index].ItemId);

                loadedItems[index] = {
                    ...loadedItems[index],
                    ...fullItem
                };
            }

            for (let item of loadedItems) {
                if (item.Quantity > item.availableQuantity) {
                    item.Quantity = item.availableQuantity;
                }
            }

            setItems(loadedItems);
        }

        func();
    }, []);

    function handleQuantityChange(item, modifier) {
        const updatedItems = items
            .map((i) => {
                if (i.ItemId === item.ItemId) {
                    const newQuant = i.Quantity + modifier;
                    if (newQuant > i.availableQuantity) {
                        newQuant = i.availableQuantity;
                    }
                    return {
                        ...i,
                        Quantity: newQuant,
                    };
                }

                return i;
            })
            .filter((i) => i.Quantity > 0);



        setItems(updatedItems);

        localStorage.setItem("cart", JSON.stringify(updatedItems));
    }

    async function handlePlaceOrder() {
        if (await tryPlaceOrder(items)) {
            setMessage("Zamówienie zostało przyjęte");
            toggleModal();
            localStorage.setItem("cart", []);
        } else {
            setMessage("Wystąpił błąd podczas składania zamówienia");
            toggleModal();
        }
    }

    return (
        <div className="checkout-page">
            <div className="checkout-container">
                <h1 className="checkout-title">CHECKOUT</h1>
                <div className="checkout-card">
                    <table className="checkout-table">
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
                                        key={item.id}
                                        className="checkout-row"
                                        onClick={() =>
                                            navigate(`/item/${item.id}`)
                                        }
                                    >
                                        <td>
                                            <img
                                                className="checkout-image"
                                                src={`${API_URL}${item.thumbnailURL}`}
                                            />
                                        </td>
                                        <td>{item.name}</td>
                                        <td>{formatPrice(item.price)}</td>
                                        <td>
                                            <div className="quantity-controls">
                                                <button
                                                    className="quantity-btn"
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleQuantityChange(
                                                            item,
                                                            -1
                                                        );
                                                    }}
                                                >
                                                    -
                                                </button>

                                                <span className="quantity-value">
                                                    {item.Quantity}
                                                </span>

                                                <button
                                                    className="quantity-btn"
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleQuantityChange(
                                                            item,
                                                            1
                                                        );
                                                    }}
                                                >
                                                    +
                                                </button>
                                            </div>
                                        </td>
                                        <td>
                                            {formatPrice(
                                                item.Quantity * item.price
                                            )}
                                        </td>
                                    </tr>
                                ))}
                        </tbody>
                    </table>
                    <div className="checkout-footer">
                        <div className="checkout-total">
                            <span className="checkout-total-label">
                                Łączna kwota
                            </span>
                            <span className="checkout-total-price">
                                {formatPrice(
                                    items?.reduce(
                                        (sum, item) =>
                                            sum + item.price * item.Quantity,
                                        0
                                    ) || 0
                                )}
                            </span>
                        </div>

                        <button
                            className="place-order-btn"
                            onClick={handlePlaceOrder}
                        >
                            Złóż zamówienie
                        </button>
                    </div>
                </div>
            </div>
            <ModalWindow
                visible={modalVisible}
                message={message}
                showButtons={false}
                toggleModal={toggleModal}
            />
        </div>
    );
}
