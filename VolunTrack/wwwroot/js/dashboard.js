document.addEventListener("DOMContentLoaded", async function () {
    await loadEvents('/api/events/upcoming');

    document.querySelectorAll(".confirm-button").forEach(button => {
        button.addEventListener("click", changeEventStatus);
    });
});