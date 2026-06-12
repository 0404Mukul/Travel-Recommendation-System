using AuthAPI.Data;
using AuthAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShareController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShareController (AppDbContext context)
        {
            _context = context;
        }
    }
}