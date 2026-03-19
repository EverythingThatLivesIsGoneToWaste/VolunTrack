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
        e.preventDefault();
        clientErrorDiv.style.display = "block";

        clientErrorDiv.innerHTML = "<ul><li>" + errors.join("</li><li>") + "</li></ul>";
    } else {
        clientErrorDiv.style.display = "none";
    }
};

async function loadCategories() {
    try {
        const response = await fetch('/api/categories');
        const categories = await response.json();

        const container = document.getElementById("categories-container");
        container.innerHTML = '';

        categories.forEach(cat => {
            const div = document.createElement('div');
            div.className = 'category-item';
            div.innerHTML = `
                <div class="checkbox-wrapper">
                    <div class="round">
                        <input type="checkbox" 
                               name="CategoryIds" 
                               value="${cat.id}" 
                               id="cat_${cat.id}">
                        <label for="cat_${cat.id}"></label>
                    </div>
                </div>
                <label for="cat_${cat.id}" 
                       style="background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}40; border-left: 5px solid #${cat.colorRgb.toString(16).padStart(6, '0')};">
                    ${cat.name}
                </label>
            `;
            container.appendChild(div);
        });
    } catch (error) {
        console.error('Failed to load categories:', error);
    }
}