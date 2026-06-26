document.addEventListener("DOMContentLoaded", function () {
    const userData = document.getElementById('currentUserData');

    if (userData) {
        document.querySelector('[name="FullName"]').value = userData.dataset.fullname;
        document.querySelector('[name="Phone"]').value = userData.dataset.phone;
        document.querySelector('[name="Email"]').value = userData.dataset.email;
    }
});

document.querySelector('[name="NewPassword"]').addEventListener("input", function () {
    const passwordFields = document.getElementById("password-fields");
    if (this.value.length > 0) {
        passwordFields.style.display = "block";
    } else {
        passwordFields.style.display = "none";
        document.getElementById("CurrentPassword").value = "";
        document.getElementById("ConfirmPassword").value = "";
    }
});

document.getElementById("editProfileForm").addEventListener("submit", validateForm);

function validateForm(e) {
    const errors = [];
    const fullName = document.querySelector('[name="FullName"]').value.trim();
    const phone = document.querySelector('[name="Phone"]').value.trim();
    const email = document.querySelector('[name="Email"]').value.trim();
    const newPassword = document.querySelector('[name="NewPassword"]').value;
    const confirmPassword = document.querySelector('[name="ConfirmPassword"]').value;

    if (fullName === "") {
        errors.push("Full name must be filled out");
    } else if (fullName.length < 2) {
        errors.push("Full name must be at least 2 characters");
    }

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

    if (newPassword !== "") {
        if (newPassword.length < 6) {
            errors.push("New password must be at least 6 characters");
        }
        if (newPassword !== confirmPassword) {
            errors.push("Passwords don't match");
        }
    }

    const clientErrorDiv = document.getElementById("clientErrorSummary");
    const serverErrorDiv = document.getElementById("serverErrorSummary");

    if (serverErrorDiv) serverErrorDiv.style.display = "none";

    if (errors.length > 0) {
        e.preventDefault();
        clientErrorDiv.style.display = "block";
        clientErrorDiv.innerHTML = "<ul><li>" + errors.join("</li><li>") + "</li></ul>";
    } else {
        clientErrorDiv.style.display = "none";
    }
}