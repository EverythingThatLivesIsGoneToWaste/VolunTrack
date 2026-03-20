using System.Security.Claims;
using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
using VolunTrack.Models;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;

        public EventService(
            IEventRepository eventRepository, 
            ICategoryRepository categoryRepository)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<EventDto> AddAsync(CreateEventDto model)
        {
            if (model.EndDateTime <= model.StartDateTime)
                throw new ArgumentException("End date must be after start date");

            var categories = await _categoryRepository.GetByIdsAsync(model.CategoryIds);
            if (categories.Count != model.CategoryIds.Count)
                throw new ArgumentException("Some categories not found");

            var eventEntity = new Event
            {
                Name = model.Name,
                Description = model.Description,
                Place = model.Place,
                StartDateTime = model.StartDateTime.ToUniversalTime(),
                EndDateTime = model.EndDateTime.ToUniversalTime(),
                SkillsRequired = model.SkillsRequired ?? string.Empty,
                Status = EventStatus.Draft,
                CreatedByUserId = model.CreatedByUserId,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _eventRepository.AddAsync(eventEntity);

            var eventCategories = model.CategoryIds.Select(categoryId => new EventCategory
            {
                EventId = eventEntity.Id,
                CategoryId = categoryId
            }).ToList();

            await _eventRepository.AddEventCategoriesAsync(eventCategories);

            return EventDto.FromEntity(eventEntity, categories);
        }
    }
}
