export default function stringFormatters() {
    function formatPrice(price) {
        return price.toLocaleString("pl-PL", {
            style: "currency",
            currency: "PLN",
        });
    }

    function formatItem(item) {
        const label =
            item.Name + "     " + item.Quantity + "*" + formatPrice(item.Price);
        return (
            <>
                <image src={item.PicURL} />
                <p>{label}</p>
            </>
        );
    }

    return { formatPrice, formatItem };
}
