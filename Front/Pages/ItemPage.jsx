import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import useItems from "../Hooks/useItems";
import AddToCart from "../Components/AddToCart";
import stringFormatters from "../Helpers/stringFormatters";
import { useTranslation } from "react-i18next";

import "../Styles/ItemPageStyle.css";

export default function ItemPage() {
    const API_URL = import.meta.env.VITE_API_ADDRESS;

    const { id } = useParams();
    const { GetItemById } = useItems();
    const [item, setItem] = useState();

    const { formatPrice } = stringFormatters();
    const { t } = useTranslation();

    useEffect(() => {
        const fetchItem = async () => {
            const data = await GetItemById(id);
            setItem(data);
        };

        fetchItem();
    }, [id]);

    if (!item) return <p>Loading...</p>;

    return (
        <div className="item-page">
            <div className="item-page-container">
                {/* LEFT SIDE */}

                <div className="item-main-section">
                    <div className="item-gallery-card">
                        <div className="item-gallery-glow"></div>

                        <div className="item-image-container">
                            <img
                                src={`${API_URL}${item.thumbnailURL}`}
                                alt={item.name}
                                className="item-page-image"
                            />
                        </div>
                    </div>

                    <div className="item-info-card">
                        <div className="item-category">{t("digital_product")}</div>

                        <h1 className="item-page-title">{item.name}</h1>

                        <div className="item-meta">
                            <div className="item-meta-box">
                                <span className="meta-label">{t("owner")}</span>
                                <span className="meta-value">
                                    {item.creatorName}
                                </span>
                            </div>

                            <div className="item-meta-box">
                                <span className="meta-label">ID</span>
                                <span className="meta-value">#{id}</span>
                            </div>
                        </div>

                        <div className="item-description-box">
                            <h2>{t("description")}</h2>

                            <p>{item.description}</p>
                        </div>
                    </div>
                </div>

                {/* RIGHT SIDE */}

                <div className="item-purchase-section">
                    <div className="purchase-card">
                        <div className="purchase-glow"></div>

                        <div className="price-section">
                            <span className="price-label">{t("price")}</span>
                            <span className="item-page-price">
                                {formatPrice(item.price)}
                            </span>
                        </div>

                        <div className="stock-info">
                            <span>{t("available")}</span>
                            <span
                                className="stock-amount"
                                style={{
                                    color:
                                        item.availableQuantity <= 3
                                            ? "#ff7676"
                                            : "#7effd8",
                                }}
                            >
                                {item.availableQuantity} {t("szt")}
                            </span>
                        </div>

                        <div className="purchase-divider"></div>

                        <div className="purchase-info">
                            <div className="purchase-info-row">
                                <span>{t("instant_delivery")}</span>
                                <span className="purchase-check">✓</span>
                            </div>

                            <div className="purchase-info-row">
                                <span>{t("safe_payment")}</span>
                                <span className="purchase-check">✓</span>
                            </div>

                            <div className="purchase-info-row">
                                <span>{t("premium_quality")}</span>
                                <span className="purchase-check">✓</span>
                            </div>
                        </div>

                        <div className="add-to-cart-wrapper">
                            <AddToCart item={item} />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
