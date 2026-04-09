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
                buttonHtml = `<span class="event-closed">Набор завершен</span>`;
            }

            const categoriesHtml = e.categories?.map(cat =>
                `<span class="category-badge" style="
                background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}40; 
                border-left: 3px solid #${cat.colorRgb.toString(16).padStart(6, '0')}" title="${escapeHtml(cat.description)}">
                ${cat.name}
                </span>`
                    ).join('') || '';

            const isAdmin = window.userRole === 'Administrator';
            const isCoordinator = window.userRole === 'EventCoordinator';
            const isCreator = Number(e.createdByUserId) === Number(window.currentUserId);
            const isRegionalCoordinator = window.userRole === 'RegionCoordinator';

            let statusSelectHtml = '';
            let templateHtml = '';

            let participantsButtonHtml = `
                <button class="participants-button" data-event-id="${e.id}" title="Посмотреть участников">
                    <img src="/images/ui/buttons/user.png">
                </button>
            `;
            let documentsCounterHtml = '';
            let photosUploadButtonHtml = '';

            if (isAdmin || (isCoordinator && isCreator)) {
                statusSelectHtml = `
                    <div class="status-select-section">
                        <select class="event-status-select" data-event-id="${e.id}">
                            <option value="Draft" ${e.status === 'Draft' ? 'selected' : ''}>Черновик</option>
                            <option value="Published" ${e.status === 'Published' ? 'selected' : ''}>Опубликовано</option>
                            <option value="Cancelled" ${e.status === 'Cancelled' ? 'selected' : ''}>Отменено</option>
                        </select>
                        <button class="confirm-button" title="Подтвердить смену статуса">
                            <img src="/images/ui/buttons/checkmark.png">
                        </button>
                    </div>
                `;
                templateHtml = `
                    <button class="template-button" data-event-id="${e.id}" title="Использовать как шаблон">
                        <img src="/images/ui/buttons/template.png">
                    </button>
                `;
                photosUploadButtonHtml = `
                    <button class="upload-photo-btn" data-event-id="${e.id}">Загрузить фото</button>
                    <input type="file" class="photo-input" data-event-id="${e.id}" accept="image/*" multiple style="display: none;">
                `;
                documentsCounterHtml = `
                    <span class="event-documents-count"><img src="images/ui/buttons/document.png">${e.documentsCount || 0}</span>
                `
            }

            if (isRegionalCoordinator) {
                documentsCounterHtml = `
                    <span class="event-documents-count"><img src="images/ui/buttons/document.png">${e.documentsCount || 0}</span>
                `
            }

            let documentsHtml = '';

            if (isAdmin || (isCoordinator && isCreator) || isRegionalCoordinator) {
                documentsHtml = `
                    <div class="event-documents">
                        <h4>Документы события</h4>
                        <div class="documents-list" id="docs-${e.id}"></div>
                        <button class="upload-doc-btn" data-event-id="${e.id}">Загрузить документ</button>
                        <input type="file" class="doc-input" data-event-id="${e.id}" accept=".pdf,.docx" style="display: none;">
                    </div>
                `;
            }

            div.innerHTML = `
                <div class="event-header">
                    <h3 class="event-name">${escapeHtml(e.name)}</h3>
                    <div class="header-controls">
                        ${statusSelectHtml}
                        ${templateHtml}
                    </div>
                </div>

                <span class="event-status" data-status="${e.status}">Статус: ${getStatusText(e.status)}</span>
        
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
                    <div class="footer-controls">
                        ${participantsButtonHtml}
                        ${buttonHtml}
                    </div>
                </div>

                <div class="event-media-section" id="event-media-section-${e.id}" data-creator-id="${e.createdByUserId}">
                    <div class="event-photos">
                        <h4>Фотографии события</h4>
                        <div class="photos-grid" id="photos-${e.id}"></div>
                        ${photosUploadButtonHtml}
                    </div>
                    ${documentsHtml}
                </div>

                <div class="media-controls-section">
                    <button class="toggle-media-btn" data-event-id="${e.id}">Показать медиа</button>
                    <div class="media-counters">
                        <span class="event-photos-count"><img src="images/ui/buttons/photo.png">${e.photosCount || 0}</span>
                        ${documentsCounterHtml}
                    </div>
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

function showToast(message, type, seconds = 3000) {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type} show`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), seconds);
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

function getAvatarByRole(role) {
    const avatarMap = {
        'Volunteer': '/images/avatars/volunteer.png',
        'EventCoordinator': '/images/avatars/coordinator.png',
        'RegionCoordinator': '/images/avatars/region-coordinator.png',
        'Administrator': '/images/avatars/administrator.png'
    };
    return avatarMap[role] || '/images/avatars/default.png';
}

// Opening modal
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".participants-button");
    if (!button) return;

    const eventId = button.dataset.eventId;
    const eventItem = button.closest(".user-event-item, .event-item");
    const eventName = eventItem?.querySelector(".event-name")?.textContent || "Событие";

    const modal = document.getElementById("participantsModal");
    document.getElementById("modalEventName").textContent = eventName;
    modal.style.display = "flex";

    await loadParticipants(eventId);
});

// Closing modal
document.body.addEventListener("click", (e) => {
    const closeButton = e.target.closest(".close-modal-button");
    if (closeButton) {
        document.getElementById("participantsModal").style.display = "none";
    }
});

window.addEventListener("click", (e) => {
    const modal = document.getElementById("participantsModal");
    if (e.target === modal) {
        modal.style.display = "none";
    }
});

// Loading participants for modal
async function loadParticipants(eventId) {
    const container = document.getElementById("participantsList");
    container.innerHTML = '<div class="loading">Загрузка...</div>';

    try {
        const response = await fetch(`/api/events/${eventId}/participants`);
        const participants = await response.json();

        if (participants.length === 0) {
            container.innerHTML = '<p>Нет участников</p>';
            return;
        }

        container.innerHTML = participants.map(p => `
            <div class="participant-item">
                <img src="${getAvatarByRole(p.role)}" class="participant-avatar">
                <div class="participant-info">
                    <div class="participant-name">${escapeHtml(p.fullName)}</div>
                    <div class="participant-login">@${escapeHtml(p.login)}</div>
                    <div class="participant-email">${escapeHtml(p.email)}</div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        container.innerHTML = '<p>Ошибка загрузки</p>';
        console.error(error);
    }
}