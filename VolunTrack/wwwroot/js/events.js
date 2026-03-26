document.addEventListener("DOMContentLoaded", async function () {
    await loadEvents('/api/events/my');

    document.querySelectorAll(".confirm-button").forEach(button => {
        button.addEventListener("click", changeEventStatus);
    });
});

