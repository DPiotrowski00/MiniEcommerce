import { useState, useEffect } from "react";
import useAccount from "../Hooks/useAccount";

export default function AddressForm() {
    const [country, setCountry] = useState("");
    const [postalCode, setPostalCode] = useState("");
    const [city, setCity] = useState("");
    const [street, setStreet] = useState("");
    const [buildingNumber, setBuildingNumber] = useState("");
    const [apartmentNumber, setApartmentNumber] = useState("");

    const { getAddress, setAddress, deleteAddress } = useAccount();

    useEffect(() => {
        const LoadAddress = async () => {
            const addr = getAddress();
            setCountry(addr.country);
            setPostalCode(addr.postalCode);
            setCity(addr.city);
            setStreet(addr.street);
            setBuildingNumber(addr.buildingNumber);
            setApartmentNumber(addr.apartmentNumber);
        };

        LoadAddress();
    });

    const updateAddress = async () => {
        const form = {
            country,
            postalCode,
            city,
            street,
            buildingNumber,
            apartmentNumber,
        };

        console.log(form);
        await setAddress(form);
    };

    return (
        <div>
            <p>
                <input
                    type="text"
                    value={country}
                    onChange={(e) => {
                        setCountry(e.target.value);
                    }}
                />
                Kraina
            </p>
            <p>
                <input
                    type="text"
                    value={postalCode}
                    onChange={(e) => {
                        setPostalCode(e.target.value);
                    }}
                />
                Poskod
            </p>
            <p>
                <input
                    type="text"
                    value={city}
                    onChange={(e) => {
                        setCity(e.target.value);
                    }}
                />
                Miasto
            </p>
            <p>
                <input
                    type="text"
                    value={street}
                    onChange={(e) => {
                        setStreet(e.target.value);
                    }}
                />
                Ulica
            </p>
            <p>
                <input
                    type="text"
                    value={buildingNumber}
                    onChange={(e) => {
                        setBuildingNumber(e.target.value);
                    }}
                />
                Numer budynku
            </p>
            <p>
                <input
                    type="text"
                    value={apartmentNumber}
                    onChange={(e) => {
                        setApartmentNumber(e.target.value);
                    }}
                />
                Numer mieszkania
            </p>
            <button onClick={updateAddress}>Aktualizuj adres</button>
            <button onClick={deleteAddress}>Usuń adres</button>
        </div>
    );
}
