document.addEventListener("DOMContentLoaded", async function () {
    await loadEvents('/api/events/upcoming');
    await loadUserEvents(`/api/users/me/events/upcoming`);

    document.querySelectorAll(".confirm-button").forEach(button => {
        button.addEventListener("click", changeEventStatus);
    });

    document.querySelectorAll(".event-status-select").forEach(select => {
        select.addEventListener("change", (e) => {
            showToast("Изменения не сохранены", "alert")
        });
    });
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".join-button");
    if (button) {
        const eventId = button.dataset.eventId;
        await joinEvent(eventId);
    }
});

document.body.addEventListener("click", async (e) => {
    const templateButton = e.target.closest(".template-button");
    if (templateButton) {
        const eventId = templateButton.dataset.eventId;
        await useAsTemplate(eventId);
    }
});

document.querySelectorAll('.toggle-option').forEach(option => {
    option.addEventListener('click', async function () {
        const type = this.dataset.type;
        const container = this.closest('.toggle-switch');

        container.querySelectorAll('.toggle-option').forEach(opt => {
            opt.classList.remove('active');
        });
        this.classList.add('active');

        container.setAttribute('data-selected', type);

        await loadUserEvents(`/api/users/me/events/${type}`);
    });
});

async function loadUserEvents(url) {
    const container = document.getElementById("events-history-container");

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
            div.className = 'user-event-item';

            const startDate = new Date(e.startDateTime).toLocaleString();
            const endDate = new Date(e.endDateTime).toLocaleString();

            const categoriesHtml = e.categories?.map(cat =>
                `<span class="category-badge" style="
                background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}40; 
                border-left: 3px solid #${cat.colorRgb.toString(16).padStart(6, '0')}" 
                title="${escapeHtml(cat.name)} — ${escapeHtml(cat.description)}">
                </span>`
            ).join('') || '';

            const isCompleted = url.includes('/completed');

            let hoursDisplayHtml = '';
            let hoursButtonHtml = '';
            if (isCompleted) {
                if (e.isHoursRecorded) {
                    let statusText = '';
                    let statusClass = '';
                    
                    if (e.participationStatus === "Rejected") {
                        statusText = 'Отклонено';
                        statusClass = 'hours-rejected';
                    } else if (e.isConfirmedByCoordinator && e.isConfirmedByLeader) {
                        statusText = 'Подтверждено';
                        statusClass = 'hours-confirmed';
                    } else if (e.isConfirmedByCoordinator || e.isConfirmedByLeader) {
                        statusText = 'Частично подтверждено';
                        statusClass = 'hours-partial';
                    } else { 
                        statusText = 'На проверке';
                        statusClass = 'hours-pending';
                    }

                    hoursDisplayHtml = `
                        <div class="hours-status ${statusClass}" title="${statusText}">
                            ${e.totalHours?.toFixed(2)} ч
                        </div>
                    `;
                } else {
                    hoursButtonHtml = `
                        <button class="record-hours-button"
                        data-event-id="${e.id}" 
                        data-start-date-time="${e.startDateTime}" 
                        data-end-date-time="${e.endDateTime}" 
                        title="Записать часы">
                            <img src="/images/ui/buttons/clock.png">
                        </button>
                    `;
                }
            }

            div.innerHTML = `
                <div class="user-event-header">
                    <div class="header-top-section">
                        <h3 class="event-name">${escapeHtml(e.name)}</h3>
                    </div>

                    <div class="participants-section">
                        <h3>${e.participantsCount || 0}</h3>
                        <button class="participants-button"
                        data-event-id="${e.id}" 
                        data-created-by-id="${e.createdByUserId}" 
                        data-start-date-time="${e.startDateTime}" 
                        data-end-date-time="${e.endDateTime}" 
                        title="Посмотреть участников">
                            <img src="/images/ui/buttons/user.png">
                        </button>
                        ${hoursButtonHtml}
                        ${hoursDisplayHtml}
                    </div>
                </div>
        
                <div class="event-content">
                    <div class="event-info">
                        <p class="event-description">${escapeHtml(e.description)}</p>
        
                        <div class="event-details">
                            <div class="event-place">Место проведения: ${escapeHtml(e.place)}</div>
                            <div class="event-datetime">${startDate} — ${endDate}</div>
                        </div>
                    </div>
                
                    <div class="user-event-categories">
                        ${categoriesHtml}
                    </div>
                </div>
            `;

            container.appendChild(div);
        });

    } catch (error) {
        container.innerHTML = `<p>Ошибка загрузки истории событий</p>`;
    }
}

// Opening modal
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".record-hours-button");
    if (!button) return;

    const modal = document.getElementById("recordTimeModal");
    const eventId = button.dataset.eventId;
    const eventStart = button.dataset.startDateTime;
    const eventEnd = button.dataset.endDateTime;
    const userId = window.currentUserId;

    modal.setAttribute('data-event-id', eventId);
    modal.setAttribute('data-user-id', userId);
    modal.setAttribute('data-event-start', eventStart);
    modal.setAttribute('data-event-end', eventEnd);

    const eventName = button.closest(".user-event-item, .event-item")
        ?.querySelector(".event-name")?.textContent || "Событие";
    document.getElementById("modalEventName").textContent = eventName;
   
    const startLocal = formatToLocalDateTime(eventStart);
    const endLocal = formatToLocalDateTime(eventEnd);

    const container = document.getElementById("hours-form");
    container.innerHTML = `
        <div class="input-group-period">
            <div class="date-group">
                <label for="inputStartDateTime">Начало</label>
                <input type="datetime-local" id="inputStartDateTime" 
                       value="${startLocal}"
                       min="${startLocal}"
                       max="${endLocal}">
            </div>

            <div class="date-group">
                <label for="inputEndDateTime">Окончание</label>
                <input type="datetime-local" id="inputEndDateTime"
                       value="${endLocal}"
                       min="${startLocal}"
                       max="${endLocal}">
            </div>
        </div>
        <button class="reset-hours-button">Сбросить</button>
        <button class="submit-hours-button">Подтвердить</button>
    `;

    const startInput = document.getElementById("inputStartDateTime");
    const endInput = document.getElementById("inputEndDateTime");

    startInput.addEventListener("change", () => {
        if (startInput.value < startLocal) {
            startInput.value = startLocal;
        }
        if (startInput.value > endLocal) {
            startInput.value = endLocal;
        }
        if (startInput.value > endInput.value) {
            startInput.value = endInput.value;
        }
    });

    endInput.addEventListener("change", () => {
        if (endInput.value > endLocal) {
            endInput.value = endLocal;
        }
        if (endInput.value < startLocal) {
            endInput.value = startLocal;
        }
        if (endInput.value < startInput.value) {
            endInput.value = startInput.value;
        }
    });

    modal.style.display = "flex";
});

// Closing modal
document.body.addEventListener("click", (e) => {
    const closeButton = e.target.closest(".close-modal-button");
    if (closeButton) {
        document.getElementById("recordTimeModal").style.display = "none";
    }
});

window.addEventListener("click", (e) => {
    const modal = document.getElementById("recordTimeModal");
    if (e.target === modal) {
        modal.style.display = "none";
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".submit-hours-button");
    if (!button) return;

    await recordHours();
});

// Reset time form
document.body.addEventListener("click", (e) => {
    const button = e.target.closest(".reset-hours-button");
    if (!button) return;

    resetHours();
});

function resetHours() {
    const startInput = document.getElementById("inputStartDateTime");
    const endInput = document.getElementById("inputEndDateTime");

    const modal = document.getElementById("recordTimeModal");

    const startDateTime = modal.getAttribute('data-event-start');
    const endDateTime = modal.getAttribute('data-event-end');

    startInput.value = formatToLocalDateTime(startDateTime);
    endInput.value = formatToLocalDateTime(endDateTime);

    showToast(`Сброшено`, "success");
}

// Time form submission
async function recordHours() {
    const modal = document.getElementById("recordTimeModal");
    
    const eventId = modal.getAttribute('data-event-id');
    const userId = modal.getAttribute('data-user-id');
    const startDateTime = document.getElementById("inputStartDateTime").value;
    const endDateTime = document.getElementById("inputEndDateTime").value;

    if (!startDateTime || !endDateTime) {
        showToast("Заполните время начала и окончания", "error");
        return;
    }

    try {
        const response = await fetch(`/api/events/${eventId}/participantion/time`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userId: parseInt(userId),
                startDateTime: new Date(startDateTime).toISOString(),
                endDateTime: new Date(endDateTime).toISOString()
            })
        });

        if (response.ok) {
            showToast(`Часы успешно отправлены на проверку`, "success");
        } else {
            const error = await response.json();
            showToast(error.message || "Ошибка", "error");
            console.error(error.message);
        }
    } catch (error) {
        showToast("Ошибка соединения", "error");
    }
}

// Search handlers
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".search-button");
    if (button) {
        search = document.getElementById("searchInput");
        if (!search) return;

        await loadEvents(`/api/events/upcoming?search=${encodeURIComponent(search.value.trim())}`);
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".reset-button");
    if (button) {
        search = document.getElementById("searchInput");
        if (!search || search.value === '') return;

        search.value = '';
        await loadEvents('/api/events/upcoming');
    }
});