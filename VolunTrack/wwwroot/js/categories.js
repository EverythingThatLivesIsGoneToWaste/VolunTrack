document.addEventListener("DOMContentLoaded", async function () {
    await loadCategoriesDetailed();
});

// Toggle category activity button handler
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".toggle-button");
    if (button) {
        const categoryId = button.dataset.categoryId;
        await toggleCategory(categoryId);
    }
});

async function toggleCategory(categoryId) {
    try {
        const response = await fetch(`api/categories/${categoryId}/toggle`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            const button = document.querySelector(`.toggle-button[data-category-id="${categoryId}"]`);

            const isActive = button.dataset.active === 'true';

            button.textContent = isActive ? 'Активировать' : 'Деактивировать';
            button.dataset.active = (!isActive).toString();
            button.classList.toggle('active');

            if (isActive) 
                showToast("Категория успешно деактивирована", "success");
            else 
                showToast("Категория успешно активирована", "success");
            
        } else {
            const result = await response.json();
            showToast(result.message, "error");
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
}

// Delete category button handler
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".delete-button");
    if (button) {
        const categoryId = button.dataset.categoryId;
        await deleteCategory(categoryId);
    }
});

async function deleteCategory(categoryId) {
    try {
        const response = await fetch(`api/categories/${categoryId}`, {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            await loadCategoriesDetailed();
            showToast("Категория успешно удалена", "success");
        } else {
            const result = await response.json();
            showToast(result.message, "error");
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
}

// Edit category button handler
document.body.addEventListener("click", (e) => {
    const editButton = e.target.closest(".edit-button");
    if (!editButton) return;

    const card = editButton.closest(".category-card");
    if (!card) return;

    const editForm = card.querySelector(".edit-fields");
    if (editForm) {
        document.querySelectorAll(".edit-fields.show").forEach(form => {
            if (form !== editForm) {
                form.classList.remove("show");
            }
        });

        editForm.classList.toggle("show");
    }
});

// Save category changes button handler
document.body.addEventListener("click", async (e) => {
    const saveButton = e.target.closest(".save-edit");
    if (!saveButton) return;

    const card = saveButton.closest(".category-card");
    if (!card) return;

    const id = card.dataset.id;
    const name = card.querySelector(".edit-name").value;
    const description = card.querySelector(".edit-description").value;
    const colorHex = card.querySelector(".edit-color").value;
    const colorRgb = parseInt(colorHex.substring(1), 16);

    try {
        const response = await fetch(`/api/categories/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id, name, description, colorRgb })
        });

        const result = await response.json();

        if (response.ok) {
            await loadCategoriesDetailed();
            showToast(`Категория "${result.name}" обновлена`, "success");
        } else {
            showToast(result.message || "Ошибка обновления", "error");
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
});

// Cancel editing button handler
document.body.addEventListener("click", (e) => {
    const cancelButton = e.target.closest(".cancel-edit");
    if (!cancelButton) return;

    const card = cancelButton.closest(".category-card");
    const editForm = card.querySelector(".edit-fields");
    if (editForm) {
        editForm.classList.toggle("show");
    }
});

// Dynamically abjusts container padding when scrollbar shows
var div = document.getElementById('categories-cards-container');

function updatePadding() {
    var hasVerticalScrollbar = div.scrollHeight > div.clientHeight;
    div.style.paddingInlineEnd = hasVerticalScrollbar ? '20px' : '0';
}

updatePadding();

var resizeObserver = new ResizeObserver(updatePadding);
resizeObserver.observe(div);

window.addEventListener('resize', updatePadding);