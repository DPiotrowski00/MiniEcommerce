import { useState, useEffect } from "react";
import stringFormatters from "../Helpers/stringFormatters";

import "../Styles/AddToCart.css";
import ModalWindow from "../Components/ModalWindow";

export default function AddToCart({ item }) {
    const [fullPrice, setFullPrice] = useState(0.0);
    const [quant, setQuant] = useState(1);

    const [modalVisible, setModalVisible] = useState(false);

    const { formatPrice } = stringFormatters();

    function toggleModal() {
        setModalVisible(!modalVisible);
    }

    useEffect(() => {
        setFullPrice(quant * item.price);
    }, [quant, item.price]);

    function AddToCart() {
        let storedCart = [];

        try {
            storedCart = JSON.parse(localStorage.getItem("cart") || "[]");
        } catch {
            storedCart = [];
        }

        for (let c of storedCart) {
            if (c.ItemId === item.id) {
                c.Quantity += quant;
                if (c.Quantity > item.availableQuantity) {
                    c.Quantity = item.availableQuantity;
                }
                localStorage.setItem("cart", JSON.stringify(storedCart));
                toggleModal();
                return;
            }
        }

        const newItem = {
            ItemId: item.id,
            Quantity: quant
        };

        if (newItem.Quantity > item.availableQuantity) {
            newItem.Quantity = item.availableQuantity;
        }

        const updatedCart = [...storedCart, newItem];
        localStorage.setItem("cart", JSON.stringify(updatedCart));
        toggleModal();
    }

    function handleQuantChange(value) {
        if (value <= 0) {
            value = 1
        }
        if (value > item.availableQuantity) {
            value = item.availableQuantity;
        }
        setQuant(value);
        setFullPrice(quant * item.price);
    }

    return (
        <div className="cart-widget">

            <div className="cart-widget-glow"></div>

            <div className="cart-header">
                <h2>Dodaj do koszyka</h2>

                <span className="cart-badge">Premium</span>
            </div>

            <div className="cart-price-section">
                <span className="cart-price-label">Łączna kwota</span>

                <span className="cart-full-price">
                    {formatPrice(fullPrice)}
                </span>
            </div>

            <div className="cart-quantity-section">
                <label className="cart-label">Ilość</label>

                <div className="quantity-box">
                    <button
                        className="quantity-btn"
                        onClick={() => handleQuantChange(quant - 1)}
                    >
                        −
                    </button>

                    <input
                        type="number"
                        min="1"
                        value={quant}
                        onChange={(e) => handleQuantChange(e.target.value)}
                        className="quantity-input"
                    />

                    <button
                        className="quantity-btn"
                        onClick={() => handleQuantChange(quant + 1)}
                    >
                        +
                    </button>
                </div>
            </div>

            <div className="cart-summary">
                <div className="cart-summary-row">
                    <span>Cena za sztukę</span>
                    <span>{formatPrice(item.price)}</span>
                </div>

                <div className="cart-summary-row">
                    <span>Ilość</span>
                    <span>{quant}</span>
                </div>
            </div>

            <button className="add-cart-btn" onClick={AddToCart}>
                Dodaj do koszyka
            </button>

            <div className="cart-footer-info">
                Natychmiastowa dostawa po zakupie
            </div>
            <ModalWindow
                visible={modalVisible}
                toggleModal={toggleModal}
                message="Artykuł został dodany do koszyka"
                showButtons={true}
            />
        </div>
    );
}
