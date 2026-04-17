import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import useItems from "../Hooks/useItems";

export default function ItemPage() {
    const { id } = useParams();
    const { GetItemById } = useItems();
    const [item, setItem] = useState();

    useEffect(() => {
        const fetchItem = async () => {
            const data = await GetItemById(id);
            setItem(data);
        };

        fetchItem();
    }, [id]);

    if (!item) return <p>Loading...</p>;

    return (
        <div>
            <h1>{item.name}</h1>
            <p>{id}</p>
            <p>{item.description}</p>
            <p>{item.price}</p>
            <p>{item.creatorName}</p>
        </div>
    );
}
