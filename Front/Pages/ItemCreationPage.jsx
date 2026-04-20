import { useState } from "react";
import useItems from "../Hooks/useItems";

export default function ItemCreationPage() {
    const [file, setFile] = useState(undefined);
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState(0.0);

    const { TryCreateItem } = useItems();

    function handleChangeFile(e) {
        setFile(e.target.files[0]);
    }

    function handleClick() {
        TryCreateItem(file, name, description, price);
    }

    return (
        <div>
            <input
                type="file"
                onChange={(e) => {
                    handleChangeFile(e);
                }}
            />
            <input
                type="text"
                value={name}
                onChange={(e) => {
                    setName(e.target.value);
                }}
            />
            <input
                type="text"
                value={description}
                onChange={(e) => {
                    setDescription(e.target.value);
                }}
            />
            <input
                type="number"
                value={price}
                onChange={(e) => {
                    setPrice(Number(e.target.value));
                }}
            />
            <button onClick={handleClick}>Utwórz</button>
        </div>
    );
}
