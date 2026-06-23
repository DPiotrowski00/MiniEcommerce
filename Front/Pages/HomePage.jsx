import useItems from "../Hooks/useItems";
import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import stringFormatters from "../Helpers/stringFormatters";

import "../Styles/HomePageStyle.css";

export default function HomePage() {
    const API_URL = import.meta.env.VITE_API_ADDRESS;

    const [searchQuery, setSearchQuery] = useState("");

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
        <>
            <div className="search-container">
                <div className="search-box">
                    <span className="search-icon">⌕</span>

                    <input
                        type="text"
                        className="search-input"
                        placeholder="Szukaj produktu..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>
            </div>

            <div className="items-container">
                {items
                    .filter(
                        (item) =>
                            item.availableQuantity > 0 &&
                            item.name
                                .toLowerCase()
                                .includes(searchQuery.toLowerCase())
                    )
                    .map((item) => (
                        <Link
                        key={item.id}
                        to={`/item/${item.id}`}
                        className="item-card-link"
                    >
                        <div className="item-card">
                            <div className="floating-dot"></div>

                            <div className="item-image-wrapper">
                                <img
                                    src={`${API_URL}${item.thumbnailURL}`}
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
        </>
    );
}
