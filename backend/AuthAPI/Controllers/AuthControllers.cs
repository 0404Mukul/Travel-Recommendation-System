using AuthAPI.Data;
using AuthAPI.DTOs;
using AuthAPI.Entities;
using AuthAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterDto dto)
        {
            var userExists =
                await _context.Users
                    .AnyAsync(x =>
                        x.Email == dto.Email);

            if (userExists)
            {
                return BadRequest(
                    "Email already exists");
            }

            PasswordHelper.CreatePasswordHash(
                dto.Password,
                out byte[] passwordHash,
                out byte[] passwordSalt);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok("User Registered");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginDto dto)
        {
            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.Email == dto.Email);

            if (user == null)
            {
                return BadRequest(
                    "Invalid credentials");
            }

            var isValid =
                PasswordHelper.VerifyPasswordHash(
                    dto.Password,
                    user.PasswordHash,
                    user.PasswordSalt);

            if (!isValid)
            {
                return BadRequest(
                    "Invalid credentials");
            }

            var token =
                JwtHelper.GenerateToken(
                    user,
                    _configuration);

            return Ok(new
            {
                token,
                user.Id,
                user.Name,
                user.Email
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok(
                User.Identity?.Name
            );
        }

        [Authorize]
        [HttpPost("share/{itineraryId}")]
        public async Task<IActionResult> CreateShareLink(int itineraryId)
        {
            var token = Guid.NewGuid().ToString().Replace("-", "");

            var link = new SharedLink
            {
                ItineraryId = itineraryId,
                PublicToken = token
            };

            _context.SharedLinks.Add(link);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                ShareUrl = $"http://localhost:5000/api/share/{token}"
            });
        }

        [HttpGet("{token}")]
        public IActionResult GetSharedItinerary(
            string token)
        {
            var shared =
                _context.SharedLinks
                    .Where(x =>
                        x.PublicToken == token)
                    .Select(x => x.Itinerary)
                    .FirstOrDefault();

            if (shared == null)
            {
                return NotFound();
            }

            return Ok(shared);
        }
    }
}