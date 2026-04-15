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
        private readonly IParticipationRepository _participationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEventRepository eventRepository, 
            ICategoryRepository categoryRepository,
            IParticipationRepository participationRepository,
            IUserRepository userRepository,
            ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _participationRepository = participationRepository;
            _userRepository = userRepository;
            _logger = logger;
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

        public async Task<EventDto> UpdateStatusAsync(int eventId, EventStatus newStatus, int userId, string userRole)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");

            var isAdmin = userRole == nameof(UserRole.Administrator);
            var isCreator = eventEntity.CreatedByUserId == userId;

            if (!isCreator && !isAdmin)
                throw new Exceptions.UnauthorizedAccessException("You don't have permission to change this event status");

            eventEntity.Status = newStatus;
            await _eventRepository.UpdateAsync(eventEntity);

            return EventDto.FromEntity(eventEntity);
        }

        public async Task<List<EventDto>> GetEventsAsync(string type, int userId, string userRole)
        {
            List<Event> events;

            if (type == "upcoming")
            {
                events = await _eventRepository.GetUpcomingAsync();
            }
            else
            {
                if (userRole == nameof(UserRole.EventCoordinator))
                {
                    events = await _eventRepository.GetByCoordinatorIdAsync(userId);
                }
                else
                {
                    events = await _eventRepository.GetAllAsync();
                }
            }

            var eventDtos = new List<EventDto>();
            foreach (var e in events)
            {
                var dto = EventDto.FromEntity(e);
                dto.IsJoined = await _participationRepository.ExistsAsync(userId, e.Id);
                dto.DocumentsCount = await _eventRepository.GetEventDocumentsCountAsync(e.Id);
                eventDtos.Add(dto);
            }

            return eventDtos;
        }

        public async Task<List<ParticipantDto>> GetEventParticipantsAsync(int eventId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");

            var participants = await _participationRepository.GetByEventIdAsync(eventId);

            var leaders = await _eventRepository.GetEventLeadersIdsAsync(eventId);

            return [.. participants.Select(p => ParticipantDto.FromEntity(p, leaders.Contains(p.UserId)))];
        }

        public async Task<List<EventPhotoDto>> GetEventPhotosAsync(int eventId)
        {
            _ = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");

            var photos = await _eventRepository.GetPhotosByEventIdAsync(eventId);
            var dtos = new List<EventPhotoDto>();

            foreach (var photo in photos)
            {
                var uploadedBy = photo.UploadedByUserId.HasValue
                    ? await _userRepository.GetByIdAsync(photo.UploadedByUserId.Value)
                    : null;

                dtos.Add(EventPhotoDto.FromEntity(photo, uploadedBy));
            }

            return dtos;
        }

        public async Task<EventPhotoDto> AddEventPhotoAsync(int eventId, UploadPhotoDto dto, int userId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");
            
            var user = await _userRepository.GetByIdAsync(userId);
            var isAdmin = user?.Role == UserRole.Administrator;
            var isCreator = eventEntity.CreatedByUserId == userId;
            var isLeader = await _userRepository.IsLeaderOfEventAsync(userId, eventId);

            if (!isAdmin && !isCreator && !isLeader)
                throw new Exceptions.UnauthorizedAccessException("No permission to upload photos");

            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine("wwwroot", "uploads", "events", eventId.ToString(), fileName);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Unsupported file format. Allowed: jpg, jpeg, png, gif, webp");

            if (dto.File.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5 MB limit");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var photo = new EventPhoto
            {
                EventId = eventId,
                Title = dto.Title ?? dto.File.FileName,
                FilePath = $"/uploads/events/{eventId}/{fileName}",
                PhotoType = dto.PhotoType,
                UploadedByUserId = userId,
                UploadedAtUtc = DateTime.UtcNow
            };

            await _eventRepository.AddPhotoAsync(photo);

            return EventPhotoDto.FromEntity(photo);
        }

        public async Task RemoveEventPhotoAsync(int photoId, int userId)
        {
            var photo = await _eventRepository.GetPhotoByIdAsync(photoId)
                ?? throw new NotFoundException($"Photo {photoId} not found");

            var eventEntity = await _eventRepository.GetByIdAsync(photo.EventId)
                ?? throw new NotFoundException($"Event {photo.EventId} not found");

            var user = await _userRepository.GetByIdAsync(userId);
            var isAdmin = user?.Role == UserRole.Administrator;
            var isCreator = eventEntity.CreatedByUserId == userId;
            var isLeader = await _userRepository.IsLeaderOfEventAsync(userId, photo.EventId);
            var isUploader = photo.UploadedByUserId == userId;

            if (!isAdmin && !isCreator && !isLeader && !isUploader)
                throw new Exceptions.UnauthorizedAccessException("No permission to delete this photo");

            var fullPath = Path.Combine("wwwroot", photo.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            await _eventRepository.DeletePhotoAsync(photo);
        }

        public async Task<List<DocumentDto>> GetEventDocumentsAsync(int eventId, int userId, string userRole)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");

            bool canAccess = userRole == nameof(UserRole.Administrator) ||
                 userRole == nameof(UserRole.RegionCoordinator) ||
                 (userRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == userId);

            if (!canAccess)
                throw new Exceptions.UnauthorizedAccessException("You cannot request documents related to this event");

            var documents = await _eventRepository.GetDocumentsByEventIdAsync(eventId);
            var dtos = new List<DocumentDto>();

            var userIds = documents.Select(d => d.UploadedByUserId).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
            var users = await _userRepository.GetByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.Id, u => u);

            foreach (var document in documents)
            {
                var uploadedBy = document.UploadedByUserId.HasValue
                    ? userDict.GetValueOrDefault(document.UploadedByUserId.Value)
                    : null;
                dtos.Add(DocumentDto.FromEntity(document, uploadedBy));
            }

            return dtos;
        }

        public async Task<DocumentDto> AddEventDocumentAsync(int eventId, UploadDocumentDto dto, int userId, string userRole)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");

            bool canUpload = userRole == nameof(UserRole.Administrator) ||
                userRole == nameof(UserRole.RegionCoordinator) ||
                (userRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == userId);
            
            if (!canUpload)
                throw new Exceptions.UnauthorizedAccessException("No permission to upload documents to this event");

            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine("wwwroot", "uploads", "events", eventId.ToString(), fileName);

            var allowedExtensions = new[] { ".docx", ".pdf", ".xlsx", ".xls", ".xlsm" };
            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Unsupported file format. Allowed: docx, pdf, xlsx, xls, xlsm");

            if (dto.File.Length > 20 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 20 MB limit");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var document = new Attachment
            {
                Description = dto.Description,
                FileName = dto.FileName ?? dto.File.FileName,
                FilePath = $"/uploads/events/{eventId}/{fileName}",
                EntityId = eventId,
                EntityType = EntityType.Event,
                UploadedByUserId = userId,
                UploadedAtUtc = DateTime.UtcNow
            };

            await _eventRepository.AddDocumentAsync(document);

            _logger.LogInformation("Document {FileName} uploaded to event {EventId} by user {UserId}",
                document.FileName, eventId, userId);

            return DocumentDto.FromEntity(document);
        }

        public async Task RemoveEventDocumentAsync(int documentId, int userId, string userRole)
        {
            var document = await _eventRepository.GetDocumentByIdAsync(documentId)
                ?? throw new NotFoundException($"Document {documentId} not found");

            var eventEntity = await _eventRepository.GetByIdAsync(document.EntityId)
                ?? throw new NotFoundException($"Event {document.EntityId} not found");

            bool canDelete = userRole == nameof(UserRole.Administrator) ||
                userRole == nameof(UserRole.RegionCoordinator) ||
                (userRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == userId);

            if (!canDelete)
                throw new Exceptions.UnauthorizedAccessException("No permission to delete this document");

            var fullPath = Path.Combine("wwwroot", document.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            else
            {
                _logger.LogWarning("Document file not found at path: {FilePath} for document {DocumentId}", fullPath, documentId);
            }

            await _eventRepository.DeleteDocumentAsync(document);
            _logger.LogInformation("Document {DocumentId} deleted from database by user {UserId}", documentId, userId);
        }

        public async Task<UserDto> ToggleEventLeaderAsync(int eventId, int targetUserId, int currentUserId, string currentUserRole)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new NotFoundException($"Event {eventId} not found");

            var targetUser = await _userRepository.GetByIdAsync(targetUserId)
                ?? throw new NotFoundException($"User {targetUserId} not found");

            bool canToggle = currentUserRole == nameof(UserRole.Administrator) ||
                (currentUserRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == currentUserId);

            if (!canToggle)
                throw new Exceptions.UnauthorizedAccessException("No permission to assign/remove leaders of this event");

            if (targetUser.Role == UserRole.Administrator || targetUser.Role == UserRole.EventCoordinator)
                throw new ArgumentException("Administrators and event coordinators cannot be assigned as leaders");

            var alreadyLeader = await _eventRepository.IsUserLeaderOfEventAsync(eventId, targetUserId);

            if (alreadyLeader)
            {
                await _eventRepository.RemoveLeaderAsync(targetUserId, eventId);
                return UserDto.FromEntity(targetUser);
            }

            var leadersCount = await _eventRepository.GetEventLeadersCountAsync(eventId);
            int maxLeaders = 2;
            if (leadersCount >= maxLeaders)
                throw new EventLeaderLimitExceededException(eventId, maxLeaders);

            var newAssignment = new UserLeaderAssignment
            {
                EventId = eventId,
                UserId = targetUserId,
            };

            await _eventRepository.AssignLeaderAsync(newAssignment);

            return UserDto.FromEntity(targetUser);
        }
    }
}
