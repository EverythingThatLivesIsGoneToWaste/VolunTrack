// Show/hide media section
document.body.addEventListener("click", async (e) => {
    const btn = e.target.closest(".toggle-media-btn");
    if (btn) {
        const eventId = btn.dataset.eventId;
        const mediaSection = document.getElementById(`event-media-section-${eventId}`);
        const docsSection = document.getElementById(`event-docs-section-${eventId}`);

        if (mediaSection.classList.contains("show")) {
            mediaSection.classList.remove("show");
            if (docsSection) docsSection.classList.remove("show");
            btn.textContent = "Показать медиа";
        } else {
            mediaSection.classList.add("show");
            if (docsSection) docsSection.classList.add("show");
            btn.textContent = "Скрыть медиа";
            await loadEventPhotos(eventId);
            await loadEventDocuments(eventId);
        }
    }
});

// Event photos
async function loadEventPhotos(eventId) {
    const container = document.getElementById(`photos-${eventId}`);
    if (!container) return;

    const mediaSection = document.getElementById(`event-media-section-${eventId}`);
    const eventCreatorId = parseInt(mediaSection?.dataset.creatorId);

    const canDelete = window.userRole === 'Administrator' ||
        (window.userRole === 'EventCoordinator' && parseInt(window.currentUserId) === eventCreatorId);

    try {
        const response = await fetch(`/api/events/${eventId}/photos`);
        const photos = await response.json();

        container.innerHTML = '';

        if (photos.length === 0) {
            container.innerHTML = '<p class="no-pics">Нет загруженных фотографий</p>';
            container.classList.add('empty');
            return;
        } else {
            container.classList.remove('empty');
        }

        photos.forEach(photo => {
            const imgDiv = document.createElement('div');
            imgDiv.className = 'photo-item';
            imgDiv.innerHTML = `
                <img src="${photo.filePath}" alt="${photo.title}" class="event-photo-thumb">
                ${canDelete ? `<button class="delete-photo-btn" 
                    data-event-id="${eventId}" 
                    data-photo-id="${photo.id}"
                    title="Удалить фото">
                    <img src="images/ui/buttons/delete-bin.png">
                </button>` : ''}
            `;
            container.appendChild(imgDiv);
        });
    } catch (error) {
        console.error('Failed to load photos:', error);
    }
}

document.body.addEventListener("change", async (e) => {
    const input = e.target.closest(".photo-input");
    if (!input) return;

    const eventId = input.dataset.eventId;
    const files = Array.from(input.files);

    for (const file of files) {
        const formData = new FormData();
        formData.append("file", file);

        try {
            const response = await fetch(`/api/events/${eventId}/photos`, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                await loadEventPhotos(eventId);
            } else {
                showToast("Ошибка загрузки фото", "error");
            }
        } catch (error) {
            console.error("Upload error:", error);
            showToast("Ошибка соединения", "error");
        }
    }

    input.value = '';
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".delete-photo-btn");
    if (!button) return;

    const photoId = button.dataset.photoId;
    const eventId = button.dataset.eventId;

    try {
        const response = await fetch(`/api/events/photos/${photoId}`, {
            method: "DELETE"
        });

        if (response.ok) {
            showToast("Фото успешно удалено", "success");
            await loadEventPhotos(eventId);
        } else {
            showToast(error.message || "Ошибка удаления", "error");
        }
    } catch (error) {
        console.error("Delete error:", error);
        showToast("Ошибка соединения", "error");
    }
});

document.body.addEventListener("click", (e) => {
    const btn = e.target.closest(".upload-photo-btn");
    if (btn) {
        const eventId = btn.dataset.eventId;
        const input = document.querySelector(`.photo-input[data-event-id="${eventId}"]`);
        if (input) input.click();
    }
});

// Event documents
async function loadEventDocuments(eventId) {
    const container = document.getElementById(`docs-${eventId}`);
    if (!container) return;

    try {
        const response = await fetch(`/api/events/${eventId}/documents`);
        const documents = await response.json();

        container.innerHTML = '';

        if (documents.length === 0) {
            container.innerHTML = '<p class="no-docs">Нет загруженных документов</p>';
            container.classList.add('empty');
            return;
        } else {
            container.classList.remove('empty');
        }

        documents.forEach(doc => {
            const docDiv = document.createElement('div');
            docDiv.className = 'document-item';
            docDiv.innerHTML = `
                <div class="document-info">
                    <span class="document-name">${escapeHtml(doc.fileName)}</span>
                    <span class="document-meta">
                        Загрузил: ${escapeHtml(doc.uploadedByUserName)} — 
                        ${new Date(doc.uploadedAtUtc).toLocaleString()}
                    </span>
                    ${doc.description ? `<p class="document-description">${escapeHtml(doc.description)}</p>` : ''}
                </div>
                <div class="document-actions">
                    <a href="${doc.filePath}" class="download-doc" download>Скачать</a>
                    <button 
                    class="delete-doc-btn"
                    data-document-id="${doc.id}" 
                    data-event-id="${eventId}"
                    title="Удалить документ">
                    <img src="images/ui/buttons/delete-bin.png">
                    </button>
                </div>
            `;
            container.appendChild(docDiv);
        });
    } catch (error) {
        console.error('Failed to load documents:', error);
        container.innerHTML = '<p class="error">Ошибка загрузки документов</p>';
    }
}

document.body.addEventListener("change", async (e) => {
    const input = e.target.closest(".doc-input");
    if (!input) return;

    const eventId = input.dataset.eventId;
    const files = Array.from(input.files);

    for (const file of files) {
        const formData = new FormData();
        formData.append("file", file);

        const description = prompt("Введите описание документа (необязательно):", file.name);
        formData.append("description", description !== null ? description : "");

        formData.append("fileName", file.name);

        try {
            const response = await fetch(`/api/events/${eventId}/documents`, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                showToast(`Документ "${file.name}" загружен`, "success");
                await loadEventDocuments(eventId);
            } else {
                const error = await response.json();
                showToast(error.message || "Ошибка загрузки документа", "error");
            }
        } catch (error) {
            console.error("Upload error:", error);
            showToast("Ошибка соединения", "error");
        }
    }

    input.value = '';
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".delete-doc-btn");
    if (!button) return;

    const documentId = button.dataset.documentId;
    const eventId = button.dataset.eventId;

    try {
        const response = await fetch(`/api/events/documents/${documentId}`, {
            method: "DELETE"
        });

        if (response.ok) {
            showToast("Документ удалён", "success");
            await loadEventDocuments(eventId);
        } else {
            const error = await response.json();
            showToast(error.message || "Ошибка удаления", "error");
        }
    } catch (error) {
        console.error("Delete error:", error);
        showToast("Ошибка соединения", "error");
    }
});

document.body.addEventListener("click", (e) => {
    const btn = e.target.closest(".upload-doc-btn");
    if (btn) {
        const eventId = btn.dataset.eventId;
        const input = document.querySelector(`.doc-input[data-event-id="${eventId}"]`);
        if (input) input.click();
    }
});