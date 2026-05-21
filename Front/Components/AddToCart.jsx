import { useState, useEffect } from "react";
import stringFormatters from "../Helpers/stringFormatters";

import "../Styles/AddToCart.css";

export default function AddToCart({ item }) {
    const [fullPrice, setFullPrice] = useState(0.0);
    const [quant, setQuant] = useState(1);
    const [items, setItems] = useState([]);

    const { formatPrice } = stringFormatters();

    useEffect(() => {
        setItems(JSON.parse(localStorage.getItem("cart")));
        setFullPrice(quant * item.price);
    }, [quant]);

    function AddToCart() {
        const newItem = {
            ItemId: item.id,
            Quantity: quant,
            PicURL: item.thumbnailurl,
            Name: item.name,
            Price: item.price,
        };

        const storedCart = JSON.parse(localStorage.getItem("cart")) || [];
        const updatedCart = [...storedCart, newItem];
        setItems(updatedCart);
        localStorage.setItem("cart", JSON.stringify(updatedCart));
    }

    function handleQuantChange(value) {
        setQuant(value);
        setFullPrice(quant * item.price);
    }

    return (
        <div className="cart-widget">

            <div className="cart-widget-glow"></div>

            <div className="cart-header">
                <h2>Dodaj do koszyka</h2>

                <span className="cart-badge">
                    Premium
                </span>
            </div>

            <div className="cart-price-section">

                <span className="cart-price-label">
                    Łączna kwota
                </span>

                <span className="cart-full-price">
                    {formatPrice(fullPrice)}
                </span>

            </div>

            <div className="cart-quantity-section">

                <label className="cart-label">
                    Ilość
                </label>

                <div className="quantity-box">

                    <button
                        className="quantity-btn"
                        onClick={() =>
                            handleQuantChange(quant - 1)
                        }
                    >
                        −
                    </button>

                    <input
                        type="number"
                        min="1"
                        value={quant}
                        onChange={(e) =>
                            handleQuantChange(e.target.value)
                        }
                        className="quantity-input"
                    />

                    <button
                        className="quantity-btn"
                        onClick={() =>
                            handleQuantChange(quant + 1)
                        }
                    >
                        +
                    </button>

                </div>

            </div>

            <div className="cart-summary">

                <div className="cart-summary-row">
                    <span>Cena za sztukę</span>
                    <span>
                        {formatPrice(item.price)}
                    </span>
                </div>

                <div className="cart-summary-row">
                    <span>Ilość</span>
                    <span>{quant}</span>
                </div>

            </div>

            <button
                className="add-cart-btn"
                onClick={AddToCart}
            >
                Dodaj do koszyka
            </button>

            <div className="cart-footer-info">
                Natychmiastowa dostawa po zakupie
            </div>

        </div>
    );
}
