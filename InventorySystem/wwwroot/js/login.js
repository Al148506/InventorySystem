document.getElementById("loginForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Evitar el envío tradicional del formulario

    // Obtener los datos del formulario
    const formData = new FormData(this);

    try {
        // Realizar la solicitud AJAX
        const response = await fetch('/Login/ValidateLogin', { // Asegúrate de que esta URL sea correcta
            method: "POST",
            body: formData
        });

        if (response.ok) {
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
            console.error("Error en la solicitud:", response.statusText);
        }
    } catch (error) {
        console.error("Error de red:", error);
    }
});
