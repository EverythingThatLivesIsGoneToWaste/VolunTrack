document.body.addEventListener("click", async (e) => {
    const btn = e.target.closest(".generate-users-report-btn");
    if (btn) {
        const container = btn.closest(".generate-group");

        const status = (includeActive && includeBlocked) ? '' : (includeActive ? true : false);

        const fromDate = container.querySelector("#inputFromDate").value;
        const toDate = container.querySelector("#inputToDate").value;

        if (fromDate > toDate) {
            showToast("Отчет не может быть сгенерирован с указанием некорректного промежутка", "alert", 5000);
            return;
        }

        const includeActive = container.querySelector("#is-active").checked;
        const includeBlocked = container.querySelector("#is-blocked").checked;

        if (!includeActive && !includeBlocked) {
            showToast("По крайней мере один флаг должен быть отмечен", "alert");
            return;
        }

        try {
            const baseUrl = '/api/reports/users/export';

            const params = new URLSearchParams({
                isActive: status,
                fromDate: fromDate,
                toDate: toDate
            });

            const url = `${baseUrl}?${params.toString()}`;

            const response = await fetch(url, {  
                method: 'GET',  
                headers: {  
                    'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'  
                }  
            });  
 
            if (!response.ok) {  
                throw new Error(`HTTP error! Status: ${response.status}`);  
            }  
 
            const excelBlob = await response.blob();  
 
            downloadFile(excelBlob, 'users_report.xlsx');  
            showToast("Отчет скачан", "success");
        } catch (error) {
            showToast("Ошибка загрузки", "error");
            console.error('Download failed:', error);  
        }  
    }
});

document.body.addEventListener("click", async (e) => {
    const btn = e.target.closest(".generate-events-report-btn");
    if (btn) {
        const container = btn.closest(".generate-group");

        const eventStatus = container.querySelector(".event-status-select").value;

        const fromDate = container.querySelector("#inputFromDate").value;
        const toDate = container.querySelector("#inputToDate").value;

        if (fromDate > toDate) {
            showToast("Отчет не может быть сгенерирован с указанием некорректного промежутка", "alert", 5000);
            return;
        }

        try {
            const baseUrl = '/api/reports/events/export';

            const params = new URLSearchParams({
                fromDate: fromDate,
                toDate: toDate,
                status: eventStatus
            });

            const url = `${baseUrl}?${params.toString()}`;

            const response = await fetch(url, {  
                method: 'GET',  
                headers: {  
                    'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'  
                }  
            });  
 
            if (!response.ok) {  
                throw new Error(`HTTP error! Status: ${response.status}`);
            }  
 
            const excelBlob = await response.blob();  
 
            downloadFile(excelBlob, 'events_report.xlsx');  
            showToast("Отчет скачан", "success");
        } catch (error) {  
            showToast("Ошибка загрузки", "error");
            console.error('Download failed:', error);  
        }  
    }
});

document.body.addEventListener("click", async (e) => {
    const btn = e.target.closest(".generate-hours-report-btn");
    if (btn) {
        const container = btn.closest(".generate-group");

        const participationStatus = container.querySelector(".participation-status-select").value;

        const fromDate = container.querySelector("#inputFromDate").value;
        const toDate = container.querySelector("#inputToDate").value;

        if (fromDate > toDate) {
            showToast("Отчет не может быть сгенерирован с указанием некорректного промежутка", "alert", 5000);
            return;
        }

        const includeNotModerated = container.querySelector("#is-not-moderated").checked;
        const includeModerated = container.querySelector("#is-moderated").checked;

        if (!includeNotModerated && !includeModerated) {
            showToast("По крайней мере один флаг должен быть отмечен", "alert");
            return;
        }

        const moderated = (includeNotModerated && includeModerated) ? '' : (includeModerated ? true : false);

        try {
            const baseUrl = '/api/reports/hours/export';

            const params = new URLSearchParams({
                fromDate: fromDate,
                toDate: toDate,
                status: participationStatus,
                moderated: moderated
            });

            const url = `${baseUrl}?${params.toString()}`;

            const response = await fetch(url, {  
                method: 'GET',  
                headers: {  
                    'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'  
                }  
            });  
 
            if (!response.ok) {  
                throw new Error(`HTTP error! Status: ${response.status}`);
            }  
 
            const excelBlob = await response.blob();  
 
            downloadFile(excelBlob, 'hours_report.xlsx');  
            showToast("Отчет скачан", "success");
        } catch (error) {  
            showToast("Ошибка загрузки", "error");
            console.error('Download failed:', error);  
        }  
    }
});

function downloadFile(blob, filename) {  
        const blobUrl = URL.createObjectURL(blob);  
 
        const anchor = document.createElement('a');  
        anchor.href = blobUrl;  
        anchor.download = filename; 
   
        anchor.click();  
  
        URL.revokeObjectURL(blobUrl);  
} 