using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateToken(
            User user,
            IConfiguration configuration)
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.Name
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email
                )
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    configuration["Jwt:Key"]!
                )
            );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token =
                new JwtSecurityToken(
                    issuer:
                        configuration["Jwt:Issuer"],
                    audience:
                        configuration["Jwt:Audience"],
                    claims:
                        claims,
                    expires:
                        DateTime.UtcNow.AddDays(7),
                    signingCredentials:
                        credentials
                );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}