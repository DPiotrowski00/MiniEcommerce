import useItems from "../Hooks/useItems";
import { useState, useEffect } from "react";

export default function HomePage() {
    const [items, setItems] = useState([]);
    const { GetItems } = useItems();

    useEffect(() => {
        const fetchItems = async () => {
            const data = await GetItems();
            setItems(data);
        }

        fetchItems();
    });

    return (
        <div>
            {items.map((item) => (
                <div key={item.id}>
                    <h2>{item.name}</h2>
                    <p>{item.price}</p>
                    <img
                        src={"https://localhost:7153" + item.thumbnailURL}
                        alt={item.name}
                    />
                </div>
            ))}
        </div>
    );
}
