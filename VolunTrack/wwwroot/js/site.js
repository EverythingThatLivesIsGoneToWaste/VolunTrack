async function loadEvents(url) {
    const container = document.getElementById("events-container");

    try {
        const response = await fetch(url);
        const events = await response.json();

        container.innerHTML = '';

        if (events.length == 0) {
            container.innerHTML = `<p>События не найдены</p>`;
            return;
        }

        events.forEach(e => {
            const div = document.createElement('div');
            div.className = 'event-item';

            const startDate = new Date(e.startDateTime).toLocaleString();
            const endDate = new Date(e.endDateTime).toLocaleString();

            let buttonHtml = '';
            const isUpcoming = new Date(e.startDateTime) > new Date();
            if (isUpcoming) {
                if (e.isJoined) {
                    buttonHtml = `<button class="join-button joined" data-event-id="${e.id}" disabled>Вы записаны</button>`;
                } else {
                    buttonHtml = `<button class="join-button" data-event-id="${e.id}">Записаться</button>`;
                }
            } else {
                buttonHtml = `<span class="event-closed">Завершено</span>`;
            }

            const categoriesHtml = e.categories?.map(cat =>
                `<span class="category-badge" style="
                background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}40; 
                border-left: 3px solid #${cat.colorRgb.toString(16).padStart(6, '0')}">
                ${cat.name}
                </span>`
                    ).join('') || '';

            const isAdmin = window.userRole === 'Administrator';
            const isCoordinator = window.userRole === 'EventCoordinator';
            const isCreator = e.createdByUserId === window.currentUserId;

            let statusSelectHtml = '';
            let templateHtml = '';

            if (isAdmin || (isCoordinator && isCreator)) {
                templateHtml = `
                    <button class="template-button" data-event-id="${e.id}" title="Нажмите, чтобы использовать как шаблон">
                        <img src="/images/ui/buttons/template.png">
                    </button>
                `;
            }

            if (isAdmin || (isCoordinator && isCreator)) {
                statusSelectHtml = `
                    <div class="status-select-section">
                        <select class="event-status-select" data-event-id="${e.id}">
                            <option value="Draft" ${e.status === 'Draft' ? 'selected' : ''}>Черновик</option>
                            <option value="Published" ${e.status === 'Published' ? 'selected' : ''}>Опубликовано</option>
                            <option value="Cancelled" ${e.status === 'Cancelled' ? 'selected' : ''}>Отменено</option>
                        </select>
                        <button class="confirm-button"><img src="/images/ui/buttons/checkmark.png"></button>
                        ${templateHtml}
                    </div>
                `;
            }

            div.innerHTML = `
                <div class="event-header">
                    <div class="header-top-section">
                        <h3 class="event-name">${escapeHtml(e.name)}</h3>
                        ${statusSelectHtml}
                    </div>
                    
                    <span class="event-status" data-status="${e.status}">Статус: ${getStatusText(e.status)}</span>
                </div>
        
                <p class="event-description">${escapeHtml(e.description)}</p>
        
                <div class="event-details">
                    <div class="event-place">Место проведения: ${escapeHtml(e.place)}</div>
                    <div class="event-datetime">${startDate} — ${endDate}</div>
                    ${e.skillsRequired ? `<div class="event-skills">Требуются: ${escapeHtml(e.skillsRequired)}</div>` : ''}
                </div>
        
                <div class="event-categories">
                    ${categoriesHtml}
                </div>
        
                <div class="event-footer">
                    <span class="event-participants">Участников: ${e.participantsCount || 0}</span>
                    ${buttonHtml}
                </div>
            `;

            container.appendChild(div);
        });

        const lastUpdated = document.getElementById("last-updated-datetime");
        if (lastUpdated) {
            lastUpdated.textContent = new Date().toLocaleString();
        }
    } catch (error) {
        container.innerHTML = `<p>Ошибка загрузки событий</p>`;
        console.error('Failed to load events:', error);
    }
}

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

    try {
        const response = await fetch('/api/categories');
        const categories = await response.json();

        if (categories.length === 0) {
            container.innerHTML = `<p>Категории не найдены</p>`;
            return;
        }

        container.innerHTML = '';

        const isAdmin = window.userRole === 'Administrator';

        categories.forEach(cat => {
            let categoryCardActionsHtml = ''

            if (isAdmin) {
                categoryCardActionsHtml = `
                <div class="category-card-actions">
                    <button class="edit-button" data-category-id="${cat.id}">✏️</button>
                    <button class="toggle-button" data-category-id="${cat.id}" data-active="${cat.isActive}">
                        ${cat.isActive ? 'Деактивировать' : 'Активировать'}
                    </button>
                    <button class="delete-button" data-category-id="${cat.id}">🗑️</button>
                </div>
            `
            }

            const card = document.createElement('div');
            card.className = 'category-card';
            card.dataset.id = cat.id;
            card.style.borderLeft = `5px solid #${cat.colorRgb.toString(16).padStart(6, '0')}`;
            card.innerHTML = `
                <div class="category-card-header">
                    <h3>${escapeHtml(cat.name)}</h3>
                    ${categoryCardActionsHtml}
                </div>
                <p class="category-description">${escapeHtml(cat.description)}</p>
                <div class="category-color" style="background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}"></div>
                
                <div class="edit-fields" style="display: none;">
                    <input type="text" class="edit-name" value="${escapeHtml(cat.name)}">
                    <textarea class="edit-description">${escapeHtml(cat.description)}</textarea>
                    <input type="color" class="edit-color" value="#${cat.colorRgb.toString(16).padStart(6, '0')}">
                    <button class="save-edit" data-id="${cat.id}">Сохранить</button>
                    <button class="cancel-edit">Отмена</button>
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

async function changeEventStatus() {
    const button = event.currentTarget;
    const eventItem = button.closest(".event-item");
    const select = eventItem.querySelector(".event-status-select");
    const eventId = select.dataset.eventId;
    const newStatus = select.value;

    const statusSpan = eventItem.querySelector(".event-status");

    const currentStatus = statusSpan.dataset.status;
    if (newStatus === currentStatus) {
        showToast("Статус уже установлен", "alert");
        return;
    }

    try {
        const response = await fetch(`/api/events/${eventId}/status`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(newStatus)
        });

        if (response.ok) {
            
            statusSpan.textContent = `Статус: ${getStatusText(newStatus)}`;
            statusSpan.className = `event-status ${newStatus.toLowerCase()}`;
            statusSpan.dataset.status = `${newStatus}`;

            showToast("Статус события обновлён", "success");
        } else {
            const error = await response.json();
            showToast("Ошибка обновления статуса", "error");
        }
    } catch (error) {
        console.error('Failed to update event status:', error);
        showToast("Ошибка соединения", "error");
    }
}

function showToast(message, type) {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type} show`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

async function joinEvent(eventId) {
    console.log(`attempting to join event ${eventId}`);
    try {
        const response = await fetch('/api/participations', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ eventId: eventId })
        });

        const result = await response.json();

        if (response.ok) {
            showToast(result.message, "success");
            const button = document.querySelector(`.join-button[data-event-id="${eventId}"]`);
            button.textContent = "Вы записаны";
            button.disabled = true;
        } else {
            showToast(result.message, "error");
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
}

async function useAsTemplate(eventId) {
    try {
        const response = await fetch(`/api/events/${eventId}/template`);
        const eventData = await response.json();

        sessionStorage.setItem('templateEvent', JSON.stringify(eventData));

        window.location.href = '/events/add?template=true';
    } catch (error) {
        showToast("Ошибка загрузки шаблона", "error");
    }
}