using Microsoft.AspNetCore.Mvc;
using AuthAPI.Data;
using AuthAPI.Entities;
using AuthAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using AuthAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper __jwtHelper;

        public AuthController(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            __jwtHelper = jwtHelper;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Backend Working");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto data)
        {
            var user = _context.Users.FirstOrDefault(x=>
                x.Email == data.Email && x.Password == data.Password
            );

            if (user == null)
            {
                return BadRequest(new
                {
                    message = "Invalid Email or Password"
                });
            }
            var token = __jwtHelper.GenerateToken(user.Email);
            return Ok(new
            {
                message = "Login Success",
                token = token
            });
        }
        
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok(new
            {
                message = "Protected Profile Data"
            });
        }   

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupDto data)
        {
            var existingUser = _context.Users
                .FirstOrDefault(x=> x.Email == data.Email);
            
            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "Email Already Exist"
                });
            }

            var user = new User
            {
                Name = data.Name,
                Email = data.Email,
                Password = data.Password
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User Registered Successfully"
            });
        }
    }
}