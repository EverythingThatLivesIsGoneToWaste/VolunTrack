document.getElementById("createCategoryForm").addEventListener("submit", validateForm);

function validateForm(e) {
    const errors = [];
    const name = document.getElementById("inputName").value.trim();
    const description = document.getElementById("inputDescription").value.trim();
    const color = document.getElementById("inputColor").value;

    if (name === "") {
        errors.push("Name must be filled out");
    } else if (name.length < 3) {
        errors.push("Name must be at least 3 characters");
    }

    if (description === "") {
        errors.push("Description must be filled out");
    } else if (description.length < 3) {
        errors.push("Description must be at least 3 characters");
    }

    if (!color) {
        errors.push("Please select color");
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