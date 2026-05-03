using OfficeOpenXml;
using OfficeOpenXml.Style;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class ReportService : IReportService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IParticipationRepository _participationRepository;

        public ReportService(
            IUserRepository userRepository, 
            IEventRepository eventRepository,
            IParticipationRepository participationRepository)
        {
            ExcelPackage.License.SetNonCommercialPersonal("VolunTrack");
            _userRepository = userRepository;
            _eventRepository = eventRepository;
            _participationRepository = participationRepository;
        }

        public async Task<byte[]> GenerateUserReportAsync(bool? isActive, DateOnly? fromDate, DateOnly? toDate)
        {
            var users = await _userRepository.GetUsersForReportAsync(isActive, fromDate, toDate);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Users");

            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Логин";
            worksheet.Cells[1, 3].Value = "ФИО";
            worksheet.Cells[1, 4].Value = "Телефон";
            worksheet.Cells[1, 5].Value = "Почта";
            worksheet.Cells[1, 6].Value = "Роль";
            worksheet.Cells[1, 7].Value = "Статус";
            worksheet.Cells[1, 8].Value = "Зарегистрирован";
            worksheet.Cells[1, 9].Value = "Подтвержденные часы";
            worksheet.Cells[1, 10].Value = "Завершенные события";

            var headerRange = worksheet.Cells[1, 1, 1, 10];
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

            for (int i = 0; i < users.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = users[i].Id;
                worksheet.Cells[i + 2, 2].Value = users[i].Login;
                worksheet.Cells[i + 2, 3].Value = users[i].FullName;
                worksheet.Cells[i + 2, 4].Value = users[i].Phone;
                worksheet.Cells[i + 2, 5].Value = users[i].Email;
                worksheet.Cells[i + 2, 6].Value = UserDto.GetRoleName(users[i].Role);
                worksheet.Cells[i + 2, 7].Value = users[i].IsActive ? "Активен" : "Заблокирован";
                worksheet.Cells[i + 2, 8].Value = users[i].RegisteredAt.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[i + 2, 9].Value = Math.Round(users[i].TotalConfirmedHours, 2);
                worksheet.Cells[i + 2, 10].Value = users[i].CompletedEventsCount;
            }

            var totalRows = users.Count + 1;
            var totalCols = 10;

            using (var range = worksheet.Cells[1, 1, totalRows, totalCols])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> GenerateEventReportAsync(DateOnly? fromDate, DateOnly? toDate, EventStatus? status)
        {
            var events = await _eventRepository.GetEventsForReportAsync(fromDate, toDate, status);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Events");

            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Название";
            worksheet.Cells[1, 3].Value = "Описание";
            worksheet.Cells[1, 4].Value = "Место проведения";
            worksheet.Cells[1, 5].Value = "Время начала";
            worksheet.Cells[1, 6].Value = "Время завершения";
            worksheet.Cells[1, 7].Value = "Статус";
            worksheet.Cells[1, 8].Value = "Подтвержденные участия";
            worksheet.Cells[1, 9].Value = "Длится";
            worksheet.Cells[1, 10].Value = "Среднее время участия";
            worksheet.Cells[1, 11].Value = "Категории";

            var headerRange = worksheet.Cells[1, 1, 1, 11];
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

            for (int i = 0; i < events.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = events[i].Id;
                worksheet.Cells[i + 2, 2].Value = events[i].Name;
                worksheet.Cells[i + 2, 3].Value = events[i].Description;
                worksheet.Cells[i + 2, 4].Value = events[i].Place;
                worksheet.Cells[i + 2, 5].Value = events[i].StartDateTime.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[i + 2, 6].Value = events[i].EndDateTime.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[i + 2, 7].Value = EventDto.GetStatusName(events[i].Status);
                worksheet.Cells[i + 2, 8].Value = events[i].ParticipantsCount;
                worksheet.Cells[i + 2, 9].Value = Math.Round(events[i].TotalHours, 2);
                worksheet.Cells[i + 2, 10].Value = Math.Round(events[i].AverageParticipantsHours, 2);
                worksheet.Cells[i + 2, 11].Value = events[i].Categories;
            }

            var totalRows = events.Count + 1;
            var totalCols = 11;

            using (var range = worksheet.Cells[1, 1, totalRows, totalCols])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> GenerateHoursReportAsync(int? eventId, DateOnly? fromDate, DateOnly? toDate, ParticipationStatus? status, bool? moderated)
        {
            var hoursStats = await _participationRepository.GetHoursStatsForReportAsync(eventId, fromDate, toDate, status, moderated);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("HoursStats");

            worksheet.Cells[1, 1].Value = "ID События";
            worksheet.Cells[1, 2].Value = "Название";
            worksheet.Cells[1, 3].Value = "ID Пользователя";
            worksheet.Cells[1, 4].Value = "Логин";
            worksheet.Cells[1, 5].Value = "ФИО";
            worksheet.Cells[1, 6].Value = "Записанные часы";
            worksheet.Cells[1, 7].Value = "Статус посещения";
            worksheet.Cells[1, 8].Value = "Подтверждено лидером";
            worksheet.Cells[1, 9].Value = "Подтверждено координатором";
            worksheet.Cells[1, 10].Value = "Модерировано";

            var headerRange = worksheet.Cells[1, 1, 1, 10];
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

            for (int i = 0; i < hoursStats.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = hoursStats[i].EventId;
                worksheet.Cells[i + 2, 2].Value = hoursStats[i].EventName;
                worksheet.Cells[i + 2, 3].Value = hoursStats[i].UserId;
                worksheet.Cells[i + 2, 4].Value = hoursStats[i].Login;
                worksheet.Cells[i + 2, 5].Value = hoursStats[i].FullName;
                worksheet.Cells[i + 2, 6].Value = Math.Round(hoursStats[i].RecordedHours, 2);
                worksheet.Cells[i + 2, 7].Value = ParticipationDto.GetParticipationStatusName(hoursStats[i].ParticipationStatus);
                worksheet.Cells[i + 2, 8].Value = hoursStats[i].ConfirmedByLeader ? "Да" : "Нет";
                worksheet.Cells[i + 2, 9].Value = hoursStats[i].ConfirmedByCoordinator ? "Да" : "Нет";
                worksheet.Cells[i + 2, 10].Value = hoursStats[i].Moderated ? "Да" : "Нет";
            }

            var totalRows = hoursStats.Count + 1;
            var totalCols = 10;

            using (var range = worksheet.Cells[1, 1, totalRows, totalCols])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }
    }
}
