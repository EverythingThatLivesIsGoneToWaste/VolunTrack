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

            div.innerHTML = `
                <div class="user-event-header">
                    <div class="header-top-section">
                        <h3 class="event-name">${escapeHtml(e.name)}</h3>
                    </div>

                    <div class="participants-section">
                        <h3>${e.participantsCount || 0}</h3>
                        <button class="participants-button" data-event-id="${e.id}" title="Нажмите, чтобы просмотреть участников">
                            <img src="/images/ui/buttons/user.png">
                        </button>
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
    const eventItem = button.closest(".user-event-item");
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