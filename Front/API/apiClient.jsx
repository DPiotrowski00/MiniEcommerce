const BASE_URL =
    "https://localhost:7153";

const saveTokens = (data) => {
    localStorage.setItem("access-token", data.accessToken);
    localStorage.setItem("csrf-token", data.csrfToken);
};

const clearTokens = () => {
    localStorage.removeItem("access-token");
    localStorage.removeItem("csrf-token");
};

const tryRefreshToken = async () => {
    try {
        const response = await fetch(BASE_URL + "/login/refresh", {
            method: "POST",
            credentials: "include",
        });

        if (!response.ok) {
            clearTokens();
            return false;
        }

        const data = await response.json();

        saveTokens(data);

        return true;
    } catch (error) {
        console.error("Refresh token failed:", error);
        clearTokens();
        return false;
    }
};

export const apiFetch = async (url, options = {}) => {
    const csrfToken = localStorage.getItem("csrf-token");
    const accessToken = localStorage.getItem("access-token");

    const isFormData = options.body instanceof FormData;

    const headers = {
        ...(isFormData ? {} : { "Content-Type": "application/json" }),
        ...(options.headers || {}),
    };

    if (accessToken) {
        headers["Authorization"] = `Bearer ${accessToken}`;
    }

    if (csrfToken) {
        headers["X-CSRF-Token"] = encodeURIComponent(csrfToken);
    }

    let response = await fetch(BASE_URL + url, {
        ...options,
        headers,
        credentials: "include",
    });

    // access token wygasł → próbujemy refresh
    if (response.status === 401) {
        const refreshed = await tryRefreshToken();

        if (refreshed) {
            const newAccessToken = localStorage.getItem("access-token");
            const newCsrfToken = localStorage.getItem("csrf-token");

            if (newAccessToken) {
                headers["Authorization"] = `Bearer ${newAccessToken}`;
            }

            if (newCsrfToken) {
                headers["X-CSRF-Token"] = newCsrfToken;
            }

            response = await fetch(BASE_URL + url, {
                ...options,
                headers,
                credentials: "include",
            });
        }
    }

    return response;
};