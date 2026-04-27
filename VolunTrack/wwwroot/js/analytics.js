document.addEventListener("DOMContentLoaded", async function() {
    await loadStatistics();
});

async function fetchUserStats() {
    const response = await fetch('/api/analytics/stats/user');
    if (!response.ok) {
        showToast("Ошибка при получении статистики", 'error');
        throw new Error('Failed to fetch user stats'); 
    }
    return await response.json();
}

/*async function fetchAdminStats() {
    const response = await fetch('/api/analytics/stats/admin');
    if (!response.ok) {
        showToast("Ошибка при получении глобальной статистики", 'error');
        throw new Error('Failed to fetch admin stats'); 
    }
    return await response.json();
}*/

async function loadStatistics() {
    const isAdmin = window.userRole === 'Administrator';

    try {
        const userStats = await fetchUserStats();
        
        // Common data
        document.getElementById('totalConfirmedHours').textContent = parseFloat(userStats.totalConfirmedHours.toFixed(2));
        document.getElementById('totalCompletedEvents').textContent = userStats.totalCompletedEvents;
        favCategorySpan = document.getElementById('favoriteCategory');

        const colorRgb = userStats.favoriteCategory?.colorRgb || 0xe8e8e8;
        favCategorySpan.textContent = userStats.favoriteCategory?.name || '-';
        favCategorySpan.style.borderLeft = `5px solid #${colorRgb.toString(16).padStart(6, '0')}` || 'e8e8e8';
        favCategorySpan.title = userStats.favoriteCategory?.description || '';

        // Charts
        renderHoursByStatus(userStats.hoursByStatus);
        renderEventsByCategory(userStats.eventsByCategory);

        /*if (isAdmin) {
            const adminStats = await fetchAdminStats();

            document.getElementById('totalUsers').textContent = adminStats.totalUsers;
            document.getElementById('adminSection').style.display = 'block';
            
            // Admin charts
            renderRegistrationsByMonth(adminStats.registrationsByMonth);
            renderTotalHoursByCategory(adminStats.totalHoursByCategory);
            renderEventsByMonth(adminStats.eventsByMonths);
            renderEventParticipants(adminStats.eventParticipantsCount);
        }*/
    } catch (error) {
        console.error('Error loading statistics:', error);
        showToast('Ошибка загрузки статистики', 'error');
    }
}

function renderHoursByStatus(hoursByStatus) {
    const statusOrder = ['Approved', 'Pending', 'Rejected'];
    const colorMap = {
        'Approved': '#4C8C2B',
        'Pending': '#D4A017',
        'Rejected': '#8B0000'
    };
    
    const labels = statusOrder.filter(s => hoursByStatus.hasOwnProperty(s));
    const data = labels.map(label => {
        const value = hoursByStatus[label];
        return typeof value === 'number' ? parseFloat(value.toFixed(2)) : value;
    });
    const colors = labels.map(label => colorMap[label]);

    new Chart(document.getElementById("hoursByStatusChart"), {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                backgroundColor: colors,
                data: data
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'bottom' }
            }
        }
    });
}

function renderEventsByCategory(eventsByCategory) {
    const labels = Object.keys(eventsByCategory);
    const data = Object.values(eventsByCategory).map(value => value.length);

    new Chart(document.getElementById("eventsByCategoryChart"), {
        type: 'horizontalBar',
        data: {
            labels: labels,
            datasets: [{
                label: "Событий в данной категории",
                backgroundColor: ["#003A6B", "#1B5886", "#3776A1", "#5293BB", "#6EB1D6", "#89CFF1"],
                data: data
            }]
        },
        options: {
            legend: { display: false },
            responsive: true,
            title: { 
                display: false
            }
        }
    });
} 