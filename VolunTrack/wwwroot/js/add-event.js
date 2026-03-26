document.addEventListener("DOMContentLoaded", async function () {
    await loadCategories();
});

document.getElementById("createEventForm").addEventListener("submit", validateForm);

function validateForm(e) {
    const errors = [];
    const name = document.getElementById("inputName").value.trim();
    const description = document.getElementById("inputDescription").value.trim();
    const place = document.getElementById("inputPlace").value.trim();
    const startDateTime = document.getElementById("inputStartDateTime").value;
    const endDateTime = document.getElementById("inputEndDateTime").value;
    const selectedCategories = document.querySelectorAll('input[name="CategoryIds"]:checked');

    if (name === "") {
        errors.push("Name must be filled out");
    } else if (name.length < 3) {
        errors.push("Name must be at least 3 characters");
    }

    if (description === "") {
        errors.push("Description must be filled out");
    } else if (description.length < 3)
        errors.push("Description must be at least 3 characters");;

    if (place === "") {
        errors.push("Place must be filled out");
    } else if (place.length < 5) {
        errors.push("Place must be at least 5 characters");
    }

    if (startDateTime === "") {
        errors.push("Start DateTime must be filled out");
    } else if (endDateTime === "") {
        errors.push("End DateTime must be filled out");
    } else {
        const now = new Date();
        const start = new Date(startDateTime);
        const end = new Date(endDateTime);

        if (start <= now) {
            errors.push("Start date must be in the future");
        } else if (start >= end) {
            errors.push("End date must be after start date");
        }
    }

    if (selectedCategories.length === 0) {
        errors.push("Please select at least one category");
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