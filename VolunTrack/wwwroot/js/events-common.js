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
            const isLeader = e.isLeader;

            let statusSelectHtml = '';
            let templateHtml = '';

            let participantsButtonHtml = `
                <button class="participants-button"
                data-event-id="${e.id}" 
                data-created-by-id="${e.createdByUserId}"
                data-start-date-time="${e.startDateTime}" 
                data-end-date-time="${e.endDateTime}" 
                title="Посмотреть участников">
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

            if (isLeader) {
                photosUploadButtonHtml = `
                    <button class="upload-photo-btn" data-event-id="${e.id}">Загрузить фото</button>
                    <input type="file" class="photo-input" data-event-id="${e.id}" accept="image/*" multiple style="display: none;">
                `;
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

// Opening modal
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".participants-button");
    if (!button) return;

    const modal = document.getElementById("participantsModal");
    const eventStart = button.dataset.startDateTime;
    const eventEnd = button.dataset.endDateTime;

    modal.setAttribute('data-event-id', button.dataset.eventId);
    modal.setAttribute('data-created-by-id', button.dataset.createdById);
    modal.setAttribute('data-event-start', eventStart);
    modal.setAttribute('data-event-end', eventEnd);

    const eventName = button.closest(".user-event-item, .event-item")
        ?.querySelector(".event-name")?.textContent || "Событие";
    document.getElementById("modalEventName").textContent = eventName;
    modal.style.display = "flex";

    await loadParticipants();
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
async function loadParticipants() {
    const modal = document.getElementById("participantsModal");
    const eventId = modal.getAttribute('data-event-id');
    const eventStart = modal.getAttribute('data-event-start');
    const eventEnd = modal.getAttribute('data-event-end');
    const createdById = modal.getAttribute('data-created-by-id');

    const container = document.getElementById("participantsList");
    container.innerHTML = '<div class="loading">Загрузка...</div>';

    try {
        const response = await fetch(`/api/events/${eventId}/participants`);
        const participants = await response.json();

        if (participants.length === 0) {
            container.innerHTML = '<p>Нет участников</p>';
            return;
        }

        let leaderAssignmentHtml = '';
        let hoursModifyingHtml = '';

        const isAdmin = window.userRole === 'Administrator';
        const isCoordinator = window.userRole === 'EventCoordinator';
        const isCreator = Number(createdById) === Number(window.currentUserId);

        container.innerHTML = '';

        participants.forEach(p => {
            const div = document.createElement('div');
            div.className = 'participant-item';

            if (isAdmin || (isCoordinator && isCreator)) {
                const isLeader = p.isLeader;
                leaderAssignmentHtml = `
                    <button class="assign-leader-button" 
                            data-event-id="${eventId}" 
                            data-user-id="${p.userId}" 
                            title="${isLeader ? 'Убрать лидера' : 'Назначить лидера'}">
                        <img src="/images/ui/buttons/${isLeader ? 'leader.png' : 'leader_inactive.png'}">
                    </button>
                `;

                const startLocal = formatToLocalDateTime(eventStart);
                const endLocal = formatToLocalDateTime(eventEnd);

                const checkInTime = p.checkInTime ? formatToLocalDateTime(p.checkInTime) : '';
                const checkOutTime = p.checkOutTime ? formatToLocalDateTime(p.checkOutTime) : '';

                const isApproved = p.participationStatus === 'Approved';
                const isRejected = p.participationStatus === 'Rejected';

                const buttonClass = isApproved ? 'hours-confirm-button approved' : (isRejected ? 'hours-confirm-button rejected' : 'hours-confirm-button');
                const buttonText = isApproved ? 'Отклонить' : (isRejected ? 'Подтвердить' : 'Подтвердить часы');

                const statusButtonHtml = `
                    <button class="${buttonClass}" 
                            data-participation-id="${p.participationId}"
                            data-current-status="${p.participationStatus}"
                            data-is-leader="${isLeader}">
                        ${buttonText}
                    </button>
                `;

                hoursModifyingHtml = `
                    <div class="input-group-period">
                        <div class="date-group">
                            <label for="inputStartDateTime">Начало</label>
                            <input type="datetime-local"
                                id="inputStartDateTime"
                                class="start-datetime"

                                value="${checkInTime}"
                                min="${startLocal}"
                                max="${endLocal}">
                        </div>

                        <div class="date-group">
                            <label for="inputEndDateTime">Окончание</label>
                            <input type="datetime-local"
                                id="inputEndDateTime"
                                class="end-datetime"

                                value="${checkOutTime}"
                                min="${startLocal}"
                                max="${endLocal}">
                        </div>
                    </div>

                    <button class="update-hours-button"
                    data-participation-id="${p.participationId}" 
                    data-check-in-time="${checkInTime}"
                    data-check-out-time="${checkOutTime}"
                    title="Модерировать часы">
                        <img src="/images/ui/buttons/clock.png">
                    </button>

                    ${statusButtonHtml}
                `;
            }

            div.innerHTML = `
                <img src="${getAvatarByRole(p.role)}" class="participant-avatar">
                <div class="participant-info">
                    <div class="participant-name">${escapeHtml(p.fullName)}</div>
                    <div class="participant-login">@${escapeHtml(p.login)}</div>
                    <div class="participant-email">${escapeHtml(p.email)}</div>
                </div>
                ${leaderAssignmentHtml}
                ${hoursModifyingHtml}
            `

            container.appendChild(div);

            const startInput = div.querySelector("#inputStartDateTime");
            const endInput = div.querySelector("#inputEndDateTime");

            if (startInput && endInput) {
                const startLocal = formatToLocalDateTime(eventStart);
                const endLocal = formatToLocalDateTime(eventEnd);

                startInput.min = startLocal;
                startInput.max = endLocal;
                endInput.min = startLocal;
                endInput.max = endLocal;

                startInput.addEventListener("change", () => {
                    if (startInput.value < startLocal) startInput.value = startLocal;
                    if (startInput.value > endLocal) startInput.value = endLocal;
                    if (startInput.value > endInput.value) startInput.value = endInput.value;
                });

                endInput.addEventListener("change", () => {
                    if (endInput.value > endLocal) endInput.value = endLocal;
                    if (endInput.value < startLocal) endInput.value = startLocal;
                    if (endInput.value < startInput.value) endInput.value = startInput.value;
                });
            }
        });
    } catch (error) {
        container.innerHTML = '<p>Ошибка загрузки</p>';
        console.error(error.message);
    }
}

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".assign-leader-button");
    if (!button) return;

    const modal = document.getElementById("participantsModal");
    const eventId = modal.getAttribute('data-event-id');
    const userId = button.dataset.userId;

    try {
        const response = await fetch(`/api/events/${eventId}/leader`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ UserId: userId })
        });

        if (response.ok) {
            const leader = await response.json();
            showToast(`Лидерский статус пользователя ${leader.login} успешно изменен`, "success");
            await loadParticipants();
        } else {
            const error = await response.json();
            showToast(error.message || "Ошибка", "error");
            console.error(error.message);
        }
    } catch (error) {
        showToast(error.message || "Ошибка назначения/снятия лидера", "error");
        console.error(error.message);
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".update-hours-button");
    if (!button) return;

    const participantItem = button.closest(".participant-item");
    const startInput = participantItem.querySelector(".start-datetime");
    const endInput = participantItem.querySelector(".end-datetime");

    const newStartTime = startInput.value;
    const newEndTime = endInput.value;

    const participationId = button.dataset.participationId;
    const originalStartTime = startInput.defaultValue;
    const originalEndTime = endInput.defaultValue;

    if (!newStartTime || !newEndTime) {
        showToast("Заполните время начала и окончания", "error");
        return;
    }

    if (newStartTime === originalStartTime && newEndTime === originalEndTime) {
        showToast('Нечего изменять', "alert");
        return;
    }

    try {
        const response = await fetch(`/api/participations/${participationId}/hours`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                CheckInTime: newStartTime,
                CheckOutTime: newEndTime
            })
        })
        if (response.ok) {
            showToast(`Часы успешно отправлены`, "success");
            await loadParticipants();
        } else {
            const error = await response.json();
            showToast(error.message || "Ошибка", "error");
            console.error(error.message);
        }
    } catch (error) {
        showToast(error.message || "Ошибка модерирования часов", "error");
        console.error(error.message);
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".hours-confirm-button");
    if (!button) return;

    const participationId = button.dataset.participationId;
    const currentStatus = button.dataset.currentStatus;

    const isApproved = currentStatus === 'Approved';
    const action = isApproved ? 'reject' : 'confirm';

    const participantItem = button.closest(".participant-item");
    const isLeader = participantItem.dataset.isLeader === 'true';

    const isAdmin = window.userRole === 'Administrator';
    const isCreator = Number(document.getElementById("participantsModal").getAttribute('data-created-by-id')) === Number(window.currentUserId);

    let confirmByLeader = false;
    let confirmByCoordinator = false;

    if (action === 'confirm') {
        if (isAdmin) {
            confirmByLeader = true;
            confirmByCoordinator = true;
        } else if (isCreator) {
            confirmByCoordinator = true;
        } else if (isLeader) {
            confirmByLeader = true;
        }

        if (!confirmByLeader && !confirmByCoordinator) {
            showToast("У вас нет прав для подтверждения часов", "error");
            return;
        }
    }

    try {
        const url = `/api/participations/${participationId}/hours/${action}`;
        const body = action === 'confirm' ? JSON.stringify({ confirmByLeader, confirmByCoordinator }) : null;

        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: body
        });

        if (response.ok) {
            showToast(`Часы ${action === 'confirm' ? 'подтверждены' : 'отклонены'}`, "success");
            await loadParticipants();
        } else {
            const error = await response.json();
            showToast(error.message || "Ошибка", "error");
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
});