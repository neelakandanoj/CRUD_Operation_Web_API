using Azure.Core;
using CRUD_Operation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRUD_Operation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string connectionString;
        private readonly Jwtsettingscs _jwtsettings;

        public AuthController(Jwtsettingscs jwtsettings, IConfiguration configuration)
        {
            _jwtsettings = jwtsettings;
            connectionString = configuration["ConnectionStrings:SQLServerDB"] ?? "";
        }



       
        [HttpPost("token")]
        public IActionResult Token([FromBody] LoginRequest request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username and password must be provided.");
            }

            // Validate credentials (replace with your own authentication logic)
            // Example: In-memory user validation
            var user = GetUserFromDatabase(request.UserName,request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,"some_id"),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtsettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtsettings.Issuer,
                audience: _jwtsettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtsettings.ExpiryInMinutes),
                signingCredentials: creds
                );

            var response = new
            {
                UserName = user.Username,
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                RequestDate = DateTime.Now,
            };
            return Ok(response);

        }

        private User GetUserFromDatabase(string username, string password)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("ValidateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };
                        }
                    }
                }
            }

            return null; // Return null if user is not found
        }

        //private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        //{
        //    // Example: Using BCrypt to verify hashed passwords
        //    return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        //}
    }
}
