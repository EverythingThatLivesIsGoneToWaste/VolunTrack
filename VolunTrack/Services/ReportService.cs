using OfficeOpenXml;
using VolunTrack.DTO;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class ReportService : IReportService
    {
        private readonly IUserRepository _userRepository;

        public ReportService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            ExcelPackage.License.SetNonCommercialPersonal("VolunTrack");
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

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }
    }
}
