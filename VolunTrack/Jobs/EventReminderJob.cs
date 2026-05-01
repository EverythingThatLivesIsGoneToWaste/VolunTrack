using Quartz;
using VolunTrack.Enums;
using VolunTrack.Repositories;
using VolunTrack.Services;

namespace VolunTrack.Jobs
{
    public class EventReminderJob(
    IEventRepository eventRepository,
    IParticipationRepository participationRepository,
    IEmailService emailService,
    ILogger<EventReminderJob> logger) : IJob
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IParticipationRepository _participationRepository = participationRepository;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<EventReminderJob> _logger = logger;

        public async Task Execute(IJobExecutionContext context)
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var dayAfterTomorrow = tomorrow.AddDays(1);

            var eventsTomorrow = await _eventRepository.GetEventsByDateRangeAsync(tomorrow, dayAfterTomorrow);

            eventsTomorrow = eventsTomorrow.Where(e => e.Status == EventStatus.Published).ToList();

            foreach (var e in eventsTomorrow)
            {
                var participants = await _participationRepository.GetConfirmedParticipantsByEventIdAsync(e.Id);

                foreach (var p in participants)
                {
                    try
                    {
                        await _emailService.SendReminderAsync(p.Email, e.Name, e.StartDateTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send reminder to {Email} for event {EventId}", p.Email, e.Id);
                    }
                }
            }
        }
    }
}
