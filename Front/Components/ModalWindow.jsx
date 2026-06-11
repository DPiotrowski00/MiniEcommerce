import { createPortal } from "react-dom";
import { useNavigate } from "react-router-dom";
import { useEffect } from "react";

import "../Styles/ModalWindowStyle.css";

export default function ModalWindow({
    visible,
    message,
    showButtons,
    toggleModal,
}) {
    const navigate = useNavigate();

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
                        <button onClick={toggleModal}>Kontynuuj zakupy</button>
                        <button onClick={() => navigate("/checkout")}>
                            Przejdź do koszyka
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
