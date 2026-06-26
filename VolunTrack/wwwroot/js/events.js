document.addEventListener("DOMContentLoaded", async function () {
    await loadEvents('/api/events/my');

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

// Search handlers
document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".search-button");
    if (button) {
        search = document.getElementById("searchInput");
        if (!search) return;

        await loadEvents(`/api/events/my?search=${encodeURIComponent(search.value.trim())}`);
    }
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".reset-button");
    if (button) {
        search = document.getElementById("searchInput");
        if (!search || search.value === '') return;

        search.value = '';
        await loadEvents('/api/events/my');
    }
});