using VolunTrack.Enums;

namespace VolunTrack.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateUserReportAsync(bool? isActive, DateOnly? fromDate, DateOnly? toDate);
        Task<byte[]> GenerateEventReportAsync(DateOnly? fromDate, DateOnly? toDate, EventStatus? status);
    }
}
