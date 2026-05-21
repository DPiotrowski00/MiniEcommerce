import { useState, useEffect } from "react";
import stringFormatters from "../Helpers/stringFormatters";

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
        setItems([...storedCart, newItem]);
        localStorage.setItem("cart", JSON.stringify(items));
    }

    function handleQuantChange(value) {
        setQuant(value);
        setFullPrice(quant * item.price);
    }

    return (
        <div>
            <h1>Dodaj do koszyka</h1>
            <input
                type="text"
                value={quant}
                onChange={(e) => handleQuantChange(e.target.value)}
            />
            <label>{formatPrice(fullPrice)}</label>
            <button onClick={AddToCart}>Dodaj</button>
        </div>
    );
}
