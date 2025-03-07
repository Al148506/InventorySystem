document.getElementById("registerForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Evitar el envío tradicional del formulario

    // Clear previous error message
    const errorField = document.querySelector(".errorMessage");
    errorField.textContent = "";
    errorField.classList.add("d-none");

    // Obtener los campos del formulario
    const emailField = document.querySelector('input[name="UserMail"]');
    const passwordField = document.querySelector('input[name="UserPassword"]');
    const confirmPasswordField = document.querySelector('input[name="ConfirmPassword"]');
    let errorMessage = "";

    // Check fields
    if (!emailField.value.trim() || !passwordField.value.trim() || !confirmPasswordField.value.trim()) {
        errorMessage = "You must complete all the fields";
    } else if (!isValidEmail(emailField.value.trim())) {
        errorMessage = "The email address is not valid";
    } else if (passwordField.value.trim().length < 6) {
        errorMessage = "The password must be at least 6 characters long";
    } else if (passwordField.value.trim() !== confirmPasswordField.value.trim()) {
        errorMessage = "Passwords do not match";
    }
    // If there is an error, display it and stop the submission
    if (errorMessage) {
        errorField.textContent = errorMessage;
        errorField.classList.remove("d-none");
        return;
    }
    // Obtener los datos del formulario
    const formData = new FormData(this);

    try {
        // Realizar la solicitud AJAX
        const response = await fetch('/Login/Register', { // Asegúrate de que esta URL sea correcta
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const result = await response.json();

            if (result.success) {
                alert(result.message); // Muestra mensaje de éxito
                window.location.href = result.redirectUrl;
            } else {
                // Mostrar el mensaje de error
                errorMessage.textContent = result.message;
                errorMessage.classList.remove("d-none");
                grecaptcha.reset();
                
            }
        } else {
            console.error("Error en la solicitud:", response.statusText);
        }
    } catch (error) {
        console.error("Error de red:", error);
    }
});

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
