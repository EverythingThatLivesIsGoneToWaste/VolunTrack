document.getElementById("loginForm").addEventListener("submit", validateForm);

function validateForm(e) {
    const errors = [];
    const login = document.getElementById("inputLogin").value.trim();
    const password = document.getElementById("inputPassword").value;

    if (login === "") {
        errors.push("Login must be filled out");
    } else if (login.length < 3) {
        errors.push("Login must be at least 3 characters");
    }

    if (password === "") {
        errors.push("Password must be filled out");
    } else {
        if (password.length < 6) {
            errors.push("Password must be at least 6 characters");
        }
    }

    const clientErrorDiv = document.getElementById("clientErrorSummary");

    const serverErrorDiv = document.getElementById("serverErrorSummary");
    if (serverErrorDiv !== null) serverErrorDiv.style.display = "none";

    if (errors.length > 0) {
        e.preventDefault();
        clientErrorDiv.style.display = "block";

        clientErrorDiv.innerHTML = "<ul><li>" + errors.join("</li><li>") + "</li></ul>";
    } else {
        clientErrorDiv.style.display = "none";
    }
};