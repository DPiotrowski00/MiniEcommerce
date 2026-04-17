import useItems from "../Hooks/useItems";
import { useState, useEffect } from "react";
import { Link } from "react-router-dom";

export default function HomePage() {
    const [items, setItems] = useState([]);
    const { GetItems } = useItems();

    useEffect(() => {
        const fetchItems = async () => {
            const data = await GetItems();
            setItems(data);
        };

        fetchItems();
    });

    function formatPrice(price) {
        return price.toLocaleString("pl-PL", {
            style: "currency",
            currency: "PLN",
        });
    }

    return (
        <div>
            {items.map((item) => (
                <div key={item.id}>
                    <Link
                        to={`/item/${item.id}`}
                        style={{ textDecoration: "none", color: "inherit" }}
                    >
                        <div>
                            <img
                                src={`https://localhost:7153${item.thumbnailURL}`}
                            />
                            <h3>{item.name}</h3>
                            <p>{formatPrice(item.price)}</p>
                        </div>
                    </Link>
                </div>
            ))}
        </div>
    );
}
