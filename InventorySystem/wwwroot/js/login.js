﻿document.getElementById("loginForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Evitar el envío tradicional del formulario

    const formData = new FormData(this);

    try {
        const response = await fetch('/Login/ValidateLogin', {
            method: "POST",
            body: formData
        });

        const contentType = response.headers.get("content-type");
        if (response.ok) {
            if (contentType && contentType.includes("application/json")) {
                const result = await response.json();
                if (result.success) {
                    window.location.href = result.redirectUrl;
                } else {
                    const errorMessage = document.querySelector(".errorMessage");
                    if (errorMessage) {
                        errorMessage.textContent = result.message;
                        errorMessage.classList.remove("d-none");
                    }
                }
            } else {
                console.error("La respuesta no es JSON:", await response.text());
            }
        } else {
            console.error("Error en la solicitud:", response.statusText);
        }
    } catch (error) {
        console.error("Error de red:", error);
    }
});
