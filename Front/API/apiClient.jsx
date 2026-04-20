import { getCookie } from "../Helpers/getCookie";

const BASE_URL = "https://localhost:7153";

const tryRefreshToken = async () => {
    try {
        const response = await fetch(BASE_URL + "/login/refresh", {
            method: "POST",
            credentials: "include",
        });

        if (!response.ok) {
            return false;
        }

        const accessToken = await response.text();
        localStorage.setItem("access-token", accessToken);

        return true;
    } catch {
        return false;
    }
};

export const apiFetch = async (url, options) => {
    const csrfToken = getCookie("CSRF-Token");
    const accessToken = localStorage.getItem("access-token");

    const isFormData = options.body instanceof FormData;

    const headers = {
        ...(isFormData ? {} : { "Content-Type": "application/json" }),
        ...(options.headers || {}),
    };

    if (accessToken) {
        headers["Authorization"] = "Bearer " + accessToken;
    }

    if (csrfToken) {
        headers["X-CSRF-Token"] = csrfToken;
    }

    const response = await fetch(BASE_URL + url, {
        ...options,
        headers,
        credentials: "include",
    });

    if (response.status === 401) {
        if (await tryRefreshToken()) {
            return apiFetch(url, options);
        }
    }

    return response;
};
