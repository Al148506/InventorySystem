﻿document.getElementById("loginForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Evitar el envío tradicional del formulario

    const formData = new FormData(this);
    const loadingOverlay = document.createElement("div");
    loadingOverlay.classList.add("loading-overlay");
    loadingOverlay.innerHTML = `
        <div class="loading-spinner">
            <div class="spinner-border text-primary" role="status">
                <span class="sr-only">Loading...</span>
            </div>
            <p class="loading-text">Authenticating, please wait...</p>
        </div>
    `;
    document.body.appendChild(loadingOverlay); // Mostrar el loader
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
                    // Redirigir si el inicio de sesión es exitoso
                    window.location.href = result.redirectUrl;
                } else {
                    // Mostrar mensaje de error
                    const errorMessage = document.querySelector(".errorMessage");
                    if (errorMessage) {
                        errorMessage.textContent = result.message;
                        errorMessage.classList.remove("d-none");
                    }
                    if (typeof grecaptcha !== "undefined") {
                        grecaptcha.reset();
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
    } finally {
        // Ocultar el loader
        loadingOverlay.remove();
    }
});

