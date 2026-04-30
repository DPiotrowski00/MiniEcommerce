import { apiFetch } from "../API/apiClient";

export default function useAccount() {
    const getAddress = async () => {
        await apiFetch("/address", {
            method: "GET",
        });
    };

    const setAddress = async (address) => {
        await apiFetch("/address", {
            method: "PUT",
            body: JSON.stringify(address),
        });
    };

    const deleteAddress = async () => {
        const addr = {
            country: "",
            postalCode: "",
            city: "",
            street: "",
            buildingNumber: "",
            apartmentNumber: "",
        };

        await setAddress(addr);
    };

    const tryChangePassword = async (oldPass, newPass) => {
        await apiFetch("/password", {
            method: "POST",
            body: JSON.stringify({
                oldPass,
                newPass,
            }),
        });
    };

    return { getAddress, setAddress, deleteAddress, tryChangePassword };
}
