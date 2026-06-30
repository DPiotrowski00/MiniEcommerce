import { createPortal } from "react-dom";
import { useNavigate } from "react-router-dom";
import { useEffect } from "react";
import { useTranslation } from "react-i18next"

import "../Styles/ModalWindowStyle.css";

export default function ModalWindow({
    visible,
    message,
    showButtons,
    toggleModal,
}) {
    const navigate = useNavigate();
    const { t } = useTranslation();

    useEffect(() => {
        if (visible) {
            document.body.style.overflow = "hidden";
        }

        return () => {
            document.body.style.overflow = "auto";
        };
    }, [visible]);

    if (!visible) return null;

    return createPortal(
        <div className="modal-overlay">
            <div className="modal">
                <h2>{message}</h2>

                {showButtons && (
                    <div className="modal-buttons">
                        <button onClick={toggleModal}>{t("continue_shopping")}</button>
                        <button onClick={() => navigate("/checkout")}>
                            {t("go_to_cart")}
                        </button>
                    </div>
                )}
                {!showButtons && (
                    <div className="modal-buttons">
                        <button
                            onClick={() => {
                                toggleModal();
                                window.location.reload();
                            }}
                        >
                            OK
                        </button>
                    </div>
                )}
            </div>
        </div>,
        document.body
    );
}
