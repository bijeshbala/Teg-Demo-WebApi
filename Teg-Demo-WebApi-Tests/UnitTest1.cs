using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Teg_DemoApi.Services;

namespace Teg_Demo_WebApi_Tests
{
    public class EventServiceTests
    {
        private EventService _eventService;
        private Mock<ILogger<EventService>> _loggerMock;
        private IMemoryCache _cache;

        public EventServiceTests()
        {
            _loggerMock = new Mock<ILogger<EventService>>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _eventService = new EventService(_loggerMock.Object, _cache);
        }

        [Fact]
        public async Task GetAllEventsAsync_ShouldReturnAllEvents()
        {
            // Arrange

            // Act
            var result = await _eventService.GetAllEventsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() >= 1);
        }

        [Fact]
        public async Task GetEventsByVenueAsync_ShouldReturnEventsByProvidedVenueId()
        {
            // Arrange
            var venueId = 919; 

            // Act
            var result = await _eventService.GetEventsByVenueAsync(venueId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() >= 1);
        }

        [Fact]
        public async Task GetAllVenuesAsync_ShouldReturnAllVenues()
        {
            // Arrange

            // Act
            var result = await _eventService.GetAllVenuesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() >= 1);
        }

        [Fact]
        public async Task GetVenueByEventAsync_ShouldReturnVenueForValidEventId()
        {
            // Arrange
            var eventId = 10033; 

            // Act
            var result = await _eventService.GetVenueByEventAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(919, result.Id);
            Assert.Equal("The TEG Observatory", result.Name);
            Assert.Equal(150, result.Capacity);
            Assert.Equal("Auckland, New Zealand", result.Location);
        }

        [Fact]
        public async Task GetVenueByEventAsync_ShouldReturnNullForInvalidEventId()
        {
            // Arrange
            var eventId = 100;

            // Act
            var result = await _eventService.GetVenueByEventAsync(eventId);

            // Assert
            Assert.Null(result);
        }
    }
}