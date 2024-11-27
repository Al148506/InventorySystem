document.getElementById("loginForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Evitar el envío tradicional del formulario

    // Limpiar mensajes de error anteriores
    document.getElementById("emailError").classList.add("d-none");
    document.getElementById("passwordError").classList.add("d-none");

    // Obtener los campos del formulario
    const emailField = document.querySelector('input[name="UserMail"]');
    const passwordField = document.querySelector('input[name="UserPassword"]');
    const errorMessage = document.getElementById("errorMessage");

    let hasError = false;

    // Validar correo
    if (!emailField.value.trim()) {
        document.getElementById("emailError").classList.remove("d-none");
        hasError = true;
    }

    // Validar contraseña
    if (!passwordField.value.trim()) {
        document.getElementById("passwordError").classList.remove("d-none");
        hasError = true;
    }

    // Si hay errores, detener el envío
    if (hasError) {
        return;
    }

    // Obtener los datos del formulario
    const formData = new FormData(this);

    try {
        // Realizar la solicitud AJAX
        const response = await fetch('@Url.Action("ValidateLogin", "Login")', {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            // Si el servidor responde correctamente
            const result = await response.json();

            if (result.success) {
                // Redirigir si el inicio de sesión fue exitoso
                window.location.href = result.redirectUrl;
            } else {
                // Mostrar mensaje de error del servidor
                document.getElementById("emailError").textContent = result.message;
                document.getElementById("emailError").classList.remove("d-none");
                errorMessage.classList.remove("d-none");
            }
        } else {
            console.error("Error en la solicitud:", response.statusText);
        }
    } catch (error) {
        console.error("Error de red:", error);
    }
});

// Función para mostrar errores en un campo
function showFieldError(field, message) {
    let errorSpan = field.nextElementSibling;
    if (!errorSpan || !errorSpan.classList.contains("text-danger")) {
        errorSpan = document.createElement("span");
        errorSpan.classList.add("text-danger", "field-error");
        field.parentNode.appendChild(errorSpan);
    }
    errorSpan.textContent = message;
}

// Función para limpiar errores en un campo
function clearFieldError(field) {
    const errorSpan = field.nextElementSibling;
    if (errorSpan && errorSpan.classList.contains("field-error")) {
        errorSpan.remove();
    }
}

// Validación de correo electrónico
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
