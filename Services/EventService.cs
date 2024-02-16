using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Polly;
using Teg_DemoApi.Models;

namespace Teg_DemoApi.Services
{
    /// <summary>
    /// Event service
    /// </summary>
    public class EventService
    {
        #region Declaration

        private const string EventsDataUrl = "https://teg-coding-challenge.s3.ap-southeast-2.amazonaws.com/events/event-data.json";
        private const string EventsSchemaUrl = "https://teg-coding-challenge.s3.ap-southeast-2.amazonaws.com/events/event-data.schema.json";
        private readonly ILogger<EventService> _logger;
        private readonly IMemoryCache _cache;

        #endregion

        #region Ctor

        public EventService(ILogger<EventService> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all events
        /// </summary>
        /// <returns>List of events</returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            var (events, _) = await GetDataAsync();
            return events;
        }

        /// <summary>
        /// Get all events by venue
        /// </summary>
        /// <param name="venueId"></param>
        /// <returns>List of events</returns>
        public async Task<IEnumerable<Event>> GetEventsByVenueAsync(int venueId)
        {
            var (events, _) = await GetDataAsync();
            return events.Where(e => e.VenueId == venueId);
        }

        /// <summary>
        /// Get all venues
        /// </summary>
        /// <returns>List of venues.</returns>
        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            var (_, venues) = await GetDataAsync();
            return venues;
        }

        /// <summary>
        /// Get venue using event id.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns>Venue information</returns>
        public async Task<Venue> GetVenueByEventAsync(int eventId)
        {
            var (events, venues) = await GetDataAsync();
            var @event = events.FirstOrDefault(e => e.Id == eventId);
            if (@event == null)
            {
                return null;
            }
            return venues.FirstOrDefault(v => v.Id == @event.VenueId);
        }

        /// <summary>
        /// Get venues and events.
        /// </summary>
        /// <returns>List of events and venues.</returns>
        public async Task<(List<Event>, List<Venue>)> GetDataAsync()
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Failed to fetch data. Retrying in {timeSpan.TotalSeconds} seconds. Retry attempt {retryCount}");
                    });

            return await policy.ExecuteAsync(async () =>
            {
                var eventData = await DownloadEventDataAsync();
                var schemaData = await DownloadSchemaDataAsync();

                if (ValidateData(eventData, schemaData))
                {
                    var events = JsonConvert.DeserializeObject<List<Event>>(eventData["events"].ToString());
                    var venues = JsonConvert.DeserializeObject<List<Venue>>(eventData["venues"].ToString());

                    // Cache the downloaded data in memory
                    CacheEventData(events, venues);

                    return (events, venues);
                }
                else
                {
                    // Validation failed, check if cached data is available
                    var (cachedEvents, cachedVenues) = GetCachedEventData();

                    if (cachedEvents != null && cachedVenues != null)
                    {
                        return (cachedEvents, cachedVenues);
                    }

                    // Return empty lists if both downloaded and cached data are unavailable
                    return (new List<Event>(), new List<Venue>());
                }
            });
        }

        /// <summary>
        /// Download event data
        /// </summary>
        /// <returns>Event data</returns>
        /// <exception cref="Exception"></exception>
        private async Task<JObject> DownloadEventDataAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(EventsDataUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                else
                {
                    _logger.LogError($"Failed to fetch event data. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Download schema data
        /// </summary>
        /// <returns>Schema data</returns>
        /// <exception cref="Exception"></exception>
        private async Task<JObject> DownloadSchemaDataAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(EventsSchemaUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                else
                {
                    _logger.LogError($"Failed to fetch schema data. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Validate event and venue data using JSON Schema.
        /// </summary>
        /// <param name="eventData">Event data</param>
        /// <param name="schemaData">Schema data</param>
        /// <returns>Boolean value indicating validation success</returns>
        private bool ValidateData(JObject eventData, JObject schemaData)
        {
            var schema = JSchema.Parse(schemaData.ToString());

            IList<string> validationErrors = new List<string>();
            eventData.Validate(schema, (sender, args) =>
            {
                validationErrors.Add(args.ValidationError.ToString());
            });

            if (validationErrors.Count > 0)
            {
                _logger.LogError($"Event data validation failed. Errors: {string.Join(", ", validationErrors)}");
                return false;
            }

            return true;
        }

        private void CacheEventData(List<Event> events, List<Venue> venues)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10)); 

            _cache.Set("EventData", (events, venues), cacheEntryOptions);
        }

        private (List<Event>, List<Venue>) GetCachedEventData()
        {
            return _cache.Get<(List<Event>, List<Venue>)>("EventData");
        }

        #endregion
    }
}
