const originalFetch = window.fetch;
window.fetch = async function (...args) {
    const response = await originalFetch(...args);

    if (response.status === 401) {
        let reason = 'unauthorized';
        try {
            const data = await response.clone().json();
            reason = data.reason || reason;
        } catch (e) { }

        setTimeout(() => {
            window.location.href = `/Login?reason=${reason}`;
        }, 3000);

        return response;
    }

    if (response.status === 403) {
        showToast("Доступ запрещён", "error");
        return response;
    }

    return response;
};

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function (m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}

function getStatusText(status) {
    const classMap = {
        Draft: 'Черновик',
        Published: 'Опубликовано',
        InProgress: 'Идёт',
        Completed: 'Завершено',
        Cancelled: 'Отменено'
    };
    return classMap[status] || 'unknown';
}

async function loadCategories() {
    const container = document.getElementById("categories-container");

    try {
        const response = await fetch('/api/categories');
        const categories = await response.json();

        if (categories.length == 0) {
            container.innerHTML = `<p>
                Категории не найдены
                </p >
            `;
            return;
        }

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
        container.innerHTML = `<p>
        Ошибка загрузки категорий
        </p>`

        console.error('Failed to load categories:', error);
    }
}

async function loadCategoriesDetailed() {
    const container = document.getElementById("categories-cards-container");
    if (!container) return;

    try {
        const response = await fetch('/api/categories');
        const categories = await response.json();

        if (categories.length === 0) {
            container.innerHTML = `<p>Категории не найдены</p>`;
            return;
        }

        container.innerHTML = '';

        const isAdmin = window.userRole === 'Administrator';

        const userCategories = await getUserCategories();

        categories.forEach(cat => {
            const isUserCategory = userCategories.some(uc => uc.id === cat.id);

            const actionButtonHtml = `
                <button class="toggle-user-category-button" data-id="${cat.id}" data-assigned="${isUserCategory}">
                    ${isUserCategory ? 'Удалить из моих' : 'Добавить в мои'}
                </button>
            `;

            let categoryCardActionsHtml = ''

            if (isAdmin) {
                categoryCardActionsHtml = `
                <div class="category-card-actions">
                    <button class="edit-button" data-category-id="${cat.id}" title="Нажмите, чтобы показать форму редактирования">
                        <img src="/images/ui/buttons/edit-pencil.png">
                    </button>
                    <button class="toggle-button" data-category-id="${cat.id}" data-active="${cat.isActive}">
                        ${cat.isActive ? 'Активна' : 'Нективна'}
                    </button>
                    <button class="delete-button" data-category-id="${cat.id}" title="Нажмите, чтобы удалить">
                        <img src="/images/ui/buttons/delete-bin.png">
                    </button>
                </div>
            `
            }

            const card = document.createElement('div');
            card.className = 'category-card';
            card.dataset.id = cat.id;
            card.style.borderLeft = `5px solid #${cat.colorRgb.toString(16).padStart(6, '0')}`;
            card.innerHTML = `
                <div class="category-card-header">
                    <div class="header-top-section">
                        <h3>${escapeHtml(cat.name)}</h3>
                        ${actionButtonHtml}
                    </div>
                    
                    ${categoryCardActionsHtml}
                </div>
                <p class="category-description">${escapeHtml(cat.description)}</p>
                
                <div class="edit-fields">
                    <div class="text-fields-section">
                        <input type="text" class="edit-name" value="${escapeHtml(cat.name)}">
                        <textarea class="edit-description">${escapeHtml(cat.description)}</textarea>
                    </div>

                    <div class="color-input-wrapper" style="background-color: #${cat.colorRgb.toString(16).padStart(6, '0')};">
                        <input type="color" class="edit-color" value="#${cat.colorRgb.toString(16).padStart(6, '0')}">
                    </div>

                    <div class="buttons-section">
                        
                        <button class="save-edit" data-id="${cat.id}">Сохранить</button>
                        <button class="cancel-edit">Отмена</button>
                    </div>
                </div>
            `;
            container.appendChild(card);
        });

        const lastUpdated = document.getElementById("last-updated-datetime");
        if (lastUpdated) {
            lastUpdated.textContent = new Date().toLocaleString();
        }

    } catch (error) {
        container.innerHTML = `<p>Ошибка загрузки категорий</p>`;
        console.error('Failed to load categories:', error);
    }
}

async function getUserCategories() {
    try {
        const response = await fetch('/api/users/me/categories');
        const categories = await response.json();

        if (categories.length === 0) {
            return [];
        }
        return categories;
    } catch (error) {
        showToast("Некоторые данные не были загружены (категории)", "error");
    }
}

function showToast(message, type, seconds = 3000) {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type} show`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), seconds);
}

function getAvatarByRole(role) {
    const avatarMap = {
        'Volunteer': '/images/avatars/volunteer.png',
        'EventCoordinator': '/images/avatars/coordinator.png',
        'RegionCoordinator': '/images/avatars/region-coordinator.png',
        'Administrator': '/images/avatars/administrator.png'
    };
    return avatarMap[role] || '/images/avatars/default.png';
}

function formatToLocalDateTime(utcDateString) {
    const date = new Date(utcDateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
} 