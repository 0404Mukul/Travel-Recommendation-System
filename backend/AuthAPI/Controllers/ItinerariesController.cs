using AuthAPI.Data;
using AuthAPI.DTOs;
using AuthAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItinerariesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItinerariesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/itineraries/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            CreateItineraryDto dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

            var itinerary = new Itinerary
            {
                UserId = userId,
                Destination = dto.Destination,
                Budget = dto.Budget,
                Interests = dto.Interests,
                WeatherInfo = "Pending",
                Recommendations = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Itineraries.Add(itinerary);

            await _context.SaveChangesAsync();

            return Ok(itinerary);
        }

        // GET: api/itineraries/saved
        [HttpGet("saved")]
        public IActionResult GetSaved()
        {
            var userId = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

            var itineraries = _context.Itineraries
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(itineraries);
        }

        // GET: api/itineraries/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

            var itinerary = _context.Itineraries
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (itinerary == null)
            {
                return NotFound(new
                {
                    message = "Itinerary not found"
                });
            }

            return Ok(itinerary);
        }

        // DELETE: api/itineraries/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

            var itinerary =
                await _context.Itineraries
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.UserId == userId);

            if (itinerary == null)
            {
                return NotFound(new
                {
                    message = "Itinerary not found"
                });
            }

            _context.Itineraries.Remove(itinerary);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Itinerary deleted successfully"
            });
        }
    }
}