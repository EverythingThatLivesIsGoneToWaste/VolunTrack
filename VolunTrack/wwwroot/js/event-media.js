// Show/hide media section
document.body.addEventListener("click", async (e) => {
    const btn = e.target.closest(".toggle-media-btn");
    if (btn) {
        const eventId = btn.dataset.eventId;
        const section = document.getElementById(`event-media-section-${eventId}`);

        if (section.classList.contains("show")) {
            section.classList.remove("show");
            btn.textContent = "Показать медиа";
        } else {
            section.classList.add("show");
            btn.textContent = "Скрыть медиа";
            await loadEventPhotos(eventId);
        }
    }
});

async function loadEventPhotos(eventId) {
    const container = document.getElementById(`photos-${eventId}`);
    if (!container) return;

    try {
        const response = await fetch(`/api/events/${eventId}/photos`);
        const photos = await response.json();

        container.innerHTML = '';

        photos.forEach(photo => {
            const imgDiv = document.createElement('div');
            imgDiv.className = 'photo-item';
            imgDiv.innerHTML = `
                <img src="${photo.filePath}" alt="${photo.title}" class="event-photo-thumb">
                <button class="delete-photo-btn" data-event-id=${eventId} data-photo-id="${photo.id}"><img src="images/ui/buttons/delete-bin.png"></button>
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
            showToast("Ошибка загрузки фото", "error");
        }
    } catch (error) {
        console.error("Upload error:", error);
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