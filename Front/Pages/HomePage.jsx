import useItems from "../Hooks/useItems";
import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import stringFormatters from "../Helpers/stringFormatters";

import "../Styles/HomePageStyle.css";

export default function HomePage() {
    const [items, setItems] = useState([]);
    const { GetItems } = useItems();

    const { formatPrice } = stringFormatters();

    useEffect(() => {
        const fetchItems = async () => {
            const data = await GetItems();
            setItems(data);
        };

        fetchItems();
    }, []);

    return (
        <div className="items-container">
            {items.map((item) => (
                <Link
                    key={item.id}
                    to={`/item/${item.id}`}
                    className="item-card-link"
                >
                    <div className="item-card">
                        <div className="floating-dot"></div>

                        <div className="item-image-wrapper">
                            <img
                                src={`https://localhost:7153${item.thumbnailURL}`}
                                alt={item.name}
                                className="item-image"
                            />

                            <div className="item-overlay">
                                <p className="item-description">
                                    {item.description}
                                </p>
                            </div>
                        </div>

                        <div className="item-content">
                            <h3 className="item-title">{item.name}</h3>

                            <div className="item-price">
                                {formatPrice(item.price)}
                            </div>
                        </div>
                    </div>
                </Link>
            ))}
        </div>
    );
}
