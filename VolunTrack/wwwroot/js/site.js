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

            const categoriesHtml = e.categories?.map(cat =>
                `<span class="category-badge" style="
                background-color: #${cat.colorRgb.toString(16).padStart(6, '0')}40; 
                border-left: 3px solid #${cat.colorRgb.toString(16).padStart(6, '0')}">
                ${cat.name}
                </span>`
                    ).join('') || '';

                    div.innerHTML = `
                <div class="event-header">
                    <h3 class="event-name">${escapeHtml(e.name)}</h3>
                    <span class="event-status ${e.status}">${getStatusText(e.status)}</span>
                </div>
        
                <p class="event-description">${escapeHtml(e.description)}</p>
        
                <div class="event-details">
                    <div class="event-place">${escapeHtml(e.place)}</div>
                    <div class="event-datetime">${startDate} — ${endDate}</div>
                    ${e.skillsRequired ? `<div class="event-skills">Требуются: ${escapeHtml(e.skillsRequired)}</div>` : ''}
                </div>
        
                <div class="event-categories">
                    ${categoriesHtml}
                </div>
        
                <div class="event-footer">
                    <span class="event-participants">Участников: ${e.participantsCount || 0}</span>
                    <button class="join-button" data-event-id="${e.id}">Записаться</button>
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
        0: 'Черновик',
        1: 'Опубликовано',
        2: 'Идёт',
        3: 'Завершено',
        4: 'Отменено'
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