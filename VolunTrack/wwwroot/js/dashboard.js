document.addEventListener("DOMContentLoaded", async function () {
    await loadEvents('/api/events/upcoming');

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