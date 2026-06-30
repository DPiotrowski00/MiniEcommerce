import { useState } from "react";
import { useNavigate } from "react-router-dom";

import useItems from "../Hooks/useItems";

import ModalWindow from "../Components/ModalWindow";

import "../Styles/ItemCreationPageStyle.css";

import { useTranslation } from "react-i18next";

export default function ItemCreationPage() {
    const [modalVisible, setModalVisible] = useState(false);
    const [modalMessage, setModalMessage] = useState("");

    const [file, setFile] = useState(undefined);
    const [preview, setPreview] = useState(null);

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState(0.0);
    const [availableQuantity, setAvailableQuantity] = useState(0);

    const { TryCreateItem } = useItems();

    const navigate = useNavigate();
    const { t } = useTranslation();

    function toggleModal() {
        setModalVisible(!modalVisible);
    }

    function handleChangeFile(e) {
        const selectedFile = e.target.files[0];
        setFile(selectedFile);
        setPreview(URL.createObjectURL(selectedFile));
    }

    const handleClick = async() => {
        const res = await TryCreateItem(file, name, description, price, availableQuantity);
        const data = await res.json();

        if (res.ok) {
            const ItemId = data.itemId;
            setModalMessage(t("article_creation_success"));
            toggleModal();
            navigate(`/item/${ItemId}`);
        } else {
            const message = data.message;
            setModalMessage(message);
            toggleModal();
        }
    }

    return (
        <div className="item-creation-page">
            <div className="item-creation-card">
                <h1 className="item-creation-title">{t("add_new_article")}</h1>

                <div className="image-preview-container">
                    {preview ? (
                        <img
                            src={preview}
                            alt={t("preview")}
                            className="image-preview"
                        />
                    ) : (
                        <span className="image-placeholder">
                            {t("no_image_selected")}
                        </span>
                    )}
                </div>

                <input
                    className="file-input"
                    type="file"
                    accept=".jpg,.jpeg,.png"
                    onChange={handleChangeFile}
                />

                <div className="form-group">
                    <label>{t("name")}</label>
                    <input
                        type="text"
                        value={name}
                        onChange={(e) => {
                            setName(e.target.value);
                        }}
                    />
                </div>

                <div className="form-group">
                    <label>{t("description")}</label>
                    <input
                        type="text"
                        value={description}
                        onChange={(e) => {
                            setDescription(e.target.value);
                        }}
                    />
                </div>

                <div className="form-group">
                    <label>{t("value")}</label>
                    <input
                        type="number"
                        value={price}
                        onChange={(e) => {
                            setPrice(Number(e.target.value));
                        }}
                    />
                </div>

                <div className="form-group">
                    <label>{t("quantity")}</label>
                    <input
                        type="number"
                        value={availableQuantity}
                        onChange={(e) => {
                            setAvailableQuantity(Number(e.target.value));
                        }}
                    />
                </div>

                <button className="create-button" onClick={handleClick}>
                    {t("create_item")}
                </button>
            </div>
            <ModalWindow message={modalMessage} toggleModal={toggleModal} visible={modalVisible} showButtons={false} />
        </div>
    );
}
