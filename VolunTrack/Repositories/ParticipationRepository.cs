using Microsoft.EntityFrameworkCore;
using VolunTrack.Data;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.Repositories
{
    public class ParticipationRepository : IParticipationRepository
    {
        private readonly ApplicationDbContext _context;

        public ParticipationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Participation?> GetByIdAsync(int id)
        {
            return await _context.Participations.FindAsync(id);
        }

        public async Task<List<Participation>> GetByUserIdAsync(int userId)
        {
            return await _context.Participations.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<Participation?> GetByUserAndEventAsync(int userId, int eventId)
        {
            return await _context.Participations
                .FirstOrDefaultAsync(p => p.UserId == userId  && p.EventId == eventId);
        }

        public async Task<List<Participation>> GetByEventIdAsync(int eventId)
        {
            return await _context.Participations
               .Include(p => p.User)
               .Where(p => p.EventId == eventId)
               .ToListAsync();
        }

        public async Task<List<ParticipantDto>> GetConfirmedParticipantsByEventIdAsync(int eventId)
        {
            var participations = await _context.Participations
                .Where(p => p.EventId == eventId && p.Status != ParticipationStatus.Rejected)
                .Include(p => p.User)
                .ToListAsync();

            return [..participations.Select(p => ParticipantDto.FromEntity(p, false))];
        }

        public async Task AddAsync(Participation participation)
        {
            await _context.Participations.AddAsync(participation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Participation participation)
        {
            _context.Participations.Update(participation);
            await _context.SaveChangesAsync();
        }

        public async Task MarkManualCheckOutAsync(Participation participation, DateTime checkOutTime)
        {
            participation.IsManualCheckOut = true;
            participation.CheckOutTime = checkOutTime;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConfirmationAsync(Participation participation, bool byCoordinator, bool byLeader)
        {
            if (byCoordinator) participation.IsConfirmedByCoordinator = true;
            if (byLeader) participation.IsConfirmedByLeader = true;

            if (participation.IsConfirmedByCoordinator && participation.IsConfirmedByLeader)
            {
                participation.Status = ParticipationStatus.Approved;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int userId, int eventId)
        {
            return await _context.Participations
                .AnyAsync(p => p.UserId == userId && p.EventId == eventId);
        }

        public async Task<List<Event>> GetUpcomingEventsByUserIdAsync(int userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId
                            && p.Event.StartDateTime > DateTime.UtcNow
                            && p.Event.Status == EventStatus.Published)
                .Include(p => p.Event)
                    .ThenInclude(e => e.EventCategories)
                        .ThenInclude(ec => ec.Category)
                .Include(p => p.Event)
                    .ThenInclude(e => e.Participations)
                .Select(p => p.Event)
                .ToListAsync();
        }

        public async Task<List<Event>> GetCompletedEventsByUserIdAsync(int userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId
                            && p.Event.Status == EventStatus.Completed)
                .Include(p => p.Event)
                    .ThenInclude(e => e.EventCategories)
                        .ThenInclude(ec => ec.Category)
                .Include(p => p.Event)
                    .ThenInclude(e => e.Participations)
                .Select(p => p.Event)
                .ToListAsync();
        }

        public async Task<List<HourReportDto>> GetHoursStatsForReportAsync(int? eventId, DateOnly? fromDate, DateOnly? toDate, ParticipationStatus? status, bool? moderated)
        {
            var query = _context.Participations.AsQueryable();

            if (eventId.HasValue)
            {
                query = query.Where(p => p.EventId == eventId);
            }
            else
            {
                if (fromDate.HasValue)
                {
                    var fromUtc = fromDate.Value.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
                    query = query.Where(p => p.Event.StartDateTime >= fromUtc);
                }

                if (toDate.HasValue)
                {
                    var toUtc = toDate.Value.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();
                    query = query.Where(p => p.Event.EndDateTime <= toUtc);
                }
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status);
            }

            if (moderated.HasValue)
            {
                query = query.Where(p => p.HoursModerated == moderated);
            }

            var hoursStats = await query.Select(p => new HourReportDto
            {
                EventId = p.Event.Id,
                EventName = p.Event.Name,
                UserId = p.User.Id,
                Login = p.User.Login,
                FullName = p.User.FullName,
                RecordedHours = p.TotalHours,
                ParticipationStatus = p.Status,
                ConfirmedByLeader = p.IsConfirmedByLeader,
                ConfirmedByCoordinator = p.IsConfirmedByCoordinator,
                Moderated = p.HoursModerated
            })
            .OrderBy(x => x.EventId)
            .ThenBy(x => x.UserId)
            .ToListAsync();

            return hoursStats;
        }
    }
}
