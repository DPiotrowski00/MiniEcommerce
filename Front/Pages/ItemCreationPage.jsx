import { useState } from "react";
import useItems from "../Hooks/useItems";

import "../Styles/ItemCreationPageStyle.css";

export default function ItemCreationPage() {
    const [file, setFile] = useState(undefined);
    const [preview, setPreview] = useState(null);

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState(0.0);

    const { TryCreateItem } = useItems();

    function handleChangeFile(e) {
        const selectedFile = e.target.files[0];
        setFile(selectedFile);
        setPreview(URL.createObjectURL(selectedFile));
    }

    function handleClick() {
        TryCreateItem(file, name, description, price);
    }

    return (
        <div className="item-creation-page">
            <div className="item-creation-card">
                <h1 className="item-creation-title">Dodaj nowy przedmiot</h1>

                <div className="image-preview-container">
                    {preview ? (
                        <img
                            src={preview}
                            alt="Podgląd"
                            className="image-preview"
                        />
                    ) : (
                        <span className="image-placeholder">
                            Brak wybranego zdjęcia
                        </span>
                    )}
                </div>

                <input
                    className="file-input"
                    type="file"
                    accept="image/*"
                    onChange={handleChangeFile}
                />

                <div className="form-group">
                    <label>Nazwa</label>
                    <input
                        type="text"
                        value={name}
                        onChange={(e) => {
                            setName(e.target.value);
                        }}
                    />
                </div>

                <div className="form-group">
                    <label>Opis</label>
                    <input
                        type="text"
                        value={description}
                        onChange={(e) => {
                            setDescription(e.target.value);
                        }}
                    />
                </div>

                <div className="form-group">
                    <label>Wartość</label>
                    <input
                        type="number"
                        value={price}
                        onChange={(e) => {
                            setPrice(Number(e.target.value));
                        }}
                    />
                </div>

                <button className="create-button" onClick={handleClick}>
                    Utwórz przedmiot
                </button>
            </div>
        </div>
    );
}
