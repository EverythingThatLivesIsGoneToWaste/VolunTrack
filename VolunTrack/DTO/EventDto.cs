using VolunTrack.Enums;
using VolunTrack.Models;

namespace VolunTrack.DTO
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string SkillsRequired { get; set; } = string.Empty;
        public EventStatus Status { get; set; }
        public int? CreatedByUserId { get; set; }
        public int ParticipantsCount { get; set; }
        public int PhotosCount { get; set; }
        public int DocumentsCount { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsJoined { get; set; }
        public bool IsLeader { get; set; }
        public bool IsHoursRecorded { get; set; }
        public decimal? TotalHours { get; set; }
        public bool IsConfirmedByCoordinator { get; set; }
        public bool IsConfirmedByLeader { get; set; }
        public ParticipationStatus ParticipationStatus { get; set; }
        public List<CategoryDto> Categories { get; set; } = [];

        public static EventDto FromEntity(Event @event, List<Category>? categories = null)
        {
            var dto = new EventDto
            {
                Id = @event.Id,
                Name = @event.Name,
                Description = @event.Description,
                Place = @event.Place,
                StartDateTime = @event.StartDateTime,
                EndDateTime = @event.EndDateTime,
                SkillsRequired = @event.SkillsRequired,
                Status = @event.Status,
                CreatedByUserId = @event.CreatedByUserId,
                ParticipantsCount = @event.Participations?.Count ?? 0,
                PhotosCount = @event.EventPhotos?.Count ?? 0,
                MaxParticipants = @event.EstimatedParticipantsCount,
                IsJoined = false,
                IsLeader = false,
                Categories = @event.EventCategories?.Select(ec => new CategoryDto
                {
                    Id = ec.Category.Id,
                    Name = ec.Category.Name,
                    ColorRgb = ec.Category.ColorRgb,
                    Description = ec.Category.Description
                }).ToList() ?? []
            };

            return dto;
        }
    }
}
