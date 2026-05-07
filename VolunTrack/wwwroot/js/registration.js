document.addEventListener("DOMContentLoaded", async function () {
    await loadCategories();
});

document.getElementById("registerForm").addEventListener("submit", validateForm);

function validateForm(e) {
    const errors = [];
    const login = document.getElementById("inputLogin").value.trim();
    const fullName = document.getElementById("inputFullName").value.trim();
    const phone = document.getElementById("inputPhone").value.trim();
    const email = document.getElementById("inputEmail").value.trim();
    const password = document.getElementById("inputPassword").value;
    const confirm = document.getElementById("inputAgainPassword").value;
    const selectedCategories = document.querySelectorAll('input[name="CategoryIds"]:checked');

    if (login === "") {
        errors.push("Login must be filled out");
    } else if (login.length < 3) {
        errors.push("Login must be at least 3 characters");
    }

    if (fullName === "") {
        errors.push("Full name must be filled out");
    } else if (fullName.length < 2)
        errors.push("Full name must be at least 2 characters");

    if (phone === "") {
        errors.push("Phone number must be filled out");
    } else {
        const phoneRegex = /^[\+\d\s\-\(\)]{10,20}$/;
        if (!phoneRegex.test(phone)) {
            errors.push("Please enter a valid phone number");
        }
    }

    if (email === "") {
        errors.push("Email must be filled out");
    } else {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            errors.push("Please enter a valid email address");
        }
    }

    if (password === "") {
        errors.push("Password must be filled out");
    } else {
        if (password.length < 6) {
            errors.push("Password must be at least 6 characters");
        }
        if (password !== confirm) {
            errors.push("Passwords don't match");
        }
    }

    if (selectedCategories.length === 0) {
        errors.push("Please select at least one category");
    }

    const clientErrorDiv = document.getElementById("clientErrorSummary");

    const serverErrorDiv = document.getElementById("serverErrorSummary");
    if (serverErrorDiv !== null) serverErrorDiv.style.display = "none";

    if (errors.length > 0) {
        showToast("Одно или несколько полей заполнены неверно", "alert", 4000);
        e.preventDefault();
        clientErrorDiv.style.display = "block";

        clientErrorDiv.innerHTML = "<ul><li>" + errors.join("</li><li>") + "</li></ul>";
    } else {
        clientErrorDiv.style.display = "none";
    }
};