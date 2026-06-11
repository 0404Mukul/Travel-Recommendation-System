using Microsoft.AspNetCore.Mvc;
using AuthAPI.Data;
using AuthAPI.Entities;
using AuthAPI.DTOs;
using AuthAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public AuthController(AppDbContext context, Microsoft.Extensions.Configuration.IConfiguration configuration)
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

            return Ok(new
            {
                message =
                    "User registered successfully"
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginDto dto)
        {
            var user = 
                await _context.Users
                    .FirstOrDefaultAsync(
                        x=>x.Email == dto.Email
                    );
            
            if(user == null)
            {
                return BadRequest(
                    "Invalid credentials"
                );
            }

            var isValid = 
                PasswordHelper.VerifyPasswordHash(
                    dto.Password,
                    user.PasswordHash,
                    user.PasswordSalt
                );

            if (!isValid)
            {
                return BadRequest(
                    "Invalid credentials"
                );
            }

            var token = 
                JwtHelper.GenerateToken(
                    user,
                    _configuration
                );

            return Ok(new
            {
                token,
                user.Id,
                user.Name,
                user.Email
            });
        }
    }
}