namespace VolunTrack.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateUserReportAsync(bool? isActive, DateOnly? fromDate, DateOnly? toDate);
    }
}
