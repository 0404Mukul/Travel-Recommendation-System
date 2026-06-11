namespace AuthAPI.DTOs
{
    public class SignupDto
    {
        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
// [HttpPost("signup")]
// public async Task<IActionResult> Signup(SignupDto data)
// {
//     var user = new User
//     {
//         Name = data.Name,
//         Email = data.Email,
//         Password = data.Password
//     };

//     _context.Users.Add(user);

//     await _context.SaveChangesAsync();

//     return Ok(new
//     {
//         message = "User Registered Successfully"
//     });
// }