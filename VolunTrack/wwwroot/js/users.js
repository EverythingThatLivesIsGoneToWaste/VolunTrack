document.addEventListener("DOMContentLoaded", async function () {
    await loadUsers();

    document.querySelectorAll(".user-role-select").forEach(select => {
        select.addEventListener("change", (e) => {
            const originalValue = select.dataset.originalValue;
            if (select.value !== originalValue) {
                showToast("Изменения не сохранены", "alert");
            }
        });
    });
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".search-button");
    if (button) {
        search = document.getElementById("searchInput");
        if (!search) return;

        await loadUsers(search.value.trim());
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".reset-button");
    if (button) {
        search = document.getElementById("searchInput");
        if (!search) return;

        search.value = '';
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".confirm-button");
    if (!button) return;

    const userId = button.dataset.userId;
    const select = document.querySelector(`.user-role-select[data-user-id="${userId}"]`);
    const newRole = select.value;
    const originalRole = select.dataset.originalValue;

    if (newRole === originalRole) {
        showToast("Роль не изменена", "alert");
        return;
    }

    try {
        const response = await fetch(`/api/users/${userId}/role`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userRole: newRole })
        });

        const result = await response.json();

        if (response.ok) {
            showToast(`Роль пользователя ${result.login} изменена на ${getRoleName(result.role)}`, "success");
            select.dataset.originalValue = newRole;
        } else {
            showToast(result.message, "error");
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".status-toggle-button");
    if (!button) return;

    const userId = button.dataset.userId;
    const isCurrentlyActive = button.dataset.status === 'true';
    const originalText = button.textContent;

    try {
        button.disabled = true;

        const response = await fetch(`/api/users/${userId}/activity/toggle`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' }
        });

        const result = await response.json();

        if (response.ok) {
            const newIsActive = !isCurrentlyActive;

            button.textContent = newIsActive ? 'Активен' : 'Неактивен';
            button.dataset.status = newIsActive.toString();
            button.classList.toggle('active', newIsActive);

            const statusSpan = button.closest('.user-card')?.querySelector('.user-status');
            if (statusSpan) {
                statusSpan.textContent = newIsActive ? 'Активен' : 'Неактивен';
                statusSpan.classList.toggle('active', newIsActive);
            }

            showToast(result.message || `Пользователь ${result.login} ${newIsActive ? 'активирован' : 'деактивирован'}`, "success");
        } else {
            showToast(result.message || "Ошибка изменения статуса", "error");
        }
        button.disabled = false;
    } catch (error) {
        showToast("Ошибка соединения", "error");
        button.disabled = false;
    }
});

function getRoleName(role) {
    const roleMap = {
        'Volunteer': 'Волонтёр',
        'EventCoordinator': 'Координатор мероприятий',
        'RegionCoordinator': 'Региональный координатор',
        'Administrator': 'Администратор'
    };
    return roleMap[role] || 'Неизвестно';
}

async function loadUsers(search = '') {
    const container = document.getElementById("users-container");

    try {
        const url = `/api/users${search ? `?search=${encodeURIComponent(search)}` : ''}`;
        const response = await fetch(url);
        const users = await response.json();

        container.innerHTML = '';

        if (users.length == 0) {
            container.innerHTML = `<p>Пользователи не найдены</p>`;
            return;
        }

        users.forEach(u => {
            const isAdmin = u.role === 'Administrator';

            const div = document.createElement('div');
            div.className = 'user-item';

            const categoriesHtml = u.categories?.map(cat =>
                `<span class="category-badge"
                title="${cat.description}"
                style="
                background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}40; 
                border-left: 3px solid #${cat.colorRgb.toString(16).padStart(6, '0')}">
                ${cat.name}
                </span>`
            ).join('') || '';

            let statusButtonHtml = '';
            let roleSelectHtml = '';

            if (!isAdmin) {
                statusButtonHtml = `
                    <button class="status-toggle-button" 
                            data-status="${u.isActive}" 
                            data-user-id="${u.id}">
                        ${u.isActive ? 'Активен' : 'Неактивен'}
                    </button>
                `;

                roleSelectHtml = `
                    <div class="role-select-section">
                        <select class="user-role-select" data-user-id="${u.id}" data-original-value="${u.role}">
                            <option value="Volunteer" ${u.role === 'Volunteer' ? 'selected' : ''}>Волонтер</option>
                            <option value="EventCoordinator" ${u.role === 'EventCoordinator' ? 'selected' : ''}>Координатор мероприятий</option>
                            <option value="RegionCoordinator" ${u.role === 'RegionCoordinator' ? 'selected' : ''}>Региональный кординатор</option>
                        </select>
                        <button class="confirm-button" data-user-id="${u.id}"><img src="/images/ui/buttons/checkmark.png"></button>
                    </div>
                `;
            }
                
            
            div.innerHTML = `
                <div class="user-header">
                    <div class="header-top-section">
                        <h3 class="user-login">${escapeHtml(u.login)}</h3>
                        ${statusButtonHtml}
                    </div>
                    ${roleSelectHtml}
                </div>
        
                <div class="user-details">
                    <p class="user-fullname">${escapeHtml(u.fullName)}</p>
                    <div class="user-createdAtUtc">Зарегистрирован ${new Date(u.createdAtUtc).toLocaleString()}</div>
                </div>
        
                <div class="contact-details">
                    <div class="user-phone">${escapeHtml(u.phone)}</div>
                    <div class="user-email">${escapeHtml(u.email)}</div>
                </div>
        
                <div class="user-categories">
                    ${categoriesHtml}
                </div>
        
                <div class="user-footer">
                    <span class="user-participations">Участвовал в событиях: ${u.participationsCount || 0}</span>
                </div>
            `;

            container.appendChild(div);
        });

        const lastUpdated = document.getElementById("last-updated-datetime");
        if (lastUpdated) {
            lastUpdated.textContent = new Date().toLocaleString();
        }
    } catch (error) {
        container.innerHTML = `<p>Ошибка загрузки пользователей</p>`;
    }
}

// Dynamically abjusts container padding when scrollbar shows
var div = document.getElementById('users-container');

function updatePadding() {
    var hasHorizontalScrollbar = div.scrollWidth > div.clientWidth;
    div.style.paddingBottom = hasHorizontalScrollbar ? '20px' : '0';
}

updatePadding();

var resizeObserver = new ResizeObserver(updatePadding);
resizeObserver.observe(div);

window.addEventListener('resize', updatePadding);