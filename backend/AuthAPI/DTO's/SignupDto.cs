namespace AuthAPI.DTOs
{
    public class SignupDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
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