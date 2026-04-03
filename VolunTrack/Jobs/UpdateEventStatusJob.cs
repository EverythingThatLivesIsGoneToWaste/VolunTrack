using Quartz;
using VolunTrack.Enums;
using VolunTrack.Repositories;

namespace VolunTrack.Jobs
{
    public class UpdateEventStatusJob : IJob
    {
        private readonly IEventRepository _eventRepository;

        public UpdateEventStatusJob(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var events = await _eventRepository.GetEventsToUpdateStatusAsync();
            foreach (var e in events)
            {
                if (e.EndDateTime < DateTime.UtcNow && e.Status != EventStatus.Completed)
                {
                    e.Status = EventStatus.Completed;
                }
                else if (e.StartDateTime < DateTime.UtcNow && e.Status == EventStatus.Published)
                {
                    e.Status = EventStatus.InProgress;
                }

                await _eventRepository.UpdateAsync(e);
            }
        }
    }
}
