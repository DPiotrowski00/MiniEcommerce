import { UseState, UseEffect } from "react";

export default function AddToCart({ articleID, price }) {
    const [fullPrice, setFullPrice] = UseState(0.0);
    const [quant, setQuant] = UseState(1);

    UseEffect(() => {

    })

    return (
        <div>
            <h1>Dodaj do koszyka</h1>
            <label>{</label>
        </div>
    );
}