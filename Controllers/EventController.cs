using Microsoft.AspNetCore.Mvc;
using Teg_DemoApi.Models;
using Teg_DemoApi.Services;

namespace Teg_DemoApi.Controllers
{
    /// <summary>
    /// Controller for the events
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        #region Declarations

        public readonly EventService _eventService;

        #endregion

        #region Ctor

        public EventController(EventService eventService) { _eventService = eventService; }

        #endregion

        #region Methods

        /// <summary>
        /// Get list of all events
        /// </summary>
        /// <returns>List of events</returns>
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return events.ToList();
        }

        /// <summary>
        /// Get all events that is happening to a perticular venue
        /// </summary>
        /// <param name="venueId"></param>
        /// <returns>List of events</returns>
        [HttpGet("venues/{venueId}")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEventsByVenue(int venueId)
        {
            var events = await _eventService.GetEventsByVenueAsync(venueId);
            return events.ToList();
        }

        /// <summary>
        /// Get list of all venues
        /// </summary>
        /// <returns>List of venues</returns>
        [HttpGet("venues")]
        public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
        {
            var venues = await _eventService.GetAllVenuesAsync();
            return venues.ToList();
        }

        /// <summary>
        /// Get the venue that is connected to an event id
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns>Venue details</returns>
        [HttpGet("events/{eventId}")]
        public async Task<ActionResult<Venue>> GetVenueByEvent(int eventId)
        {
            var venue = await _eventService.GetVenueByEventAsync(eventId);
            if (venue == null)
            {
                return NotFound();
            }
            return venue;
        }

        #endregion
    }
}
