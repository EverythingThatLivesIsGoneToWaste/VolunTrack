document.addEventListener("DOMContentLoaded", async function () {
    await loadEvents('/api/events/upcoming');

    document.querySelectorAll(".confirm-button").forEach(button => {
        button.addEventListener("click", changeEventStatus);
    });
});

document.body.addEventListener("click", async (e) => {
    const button = e.target.closest(".join-button");
    if (button) {
        const eventId = button.dataset.eventId;
        await joinEvent(eventId);
    }
});