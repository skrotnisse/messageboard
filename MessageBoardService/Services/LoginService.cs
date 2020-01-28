using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MessageBoardService.Services
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }

    public class AppSettings
    {
        public string Secret { get; set; }
    }

    public interface ILoginService
    {
        User Authenticate(string username, string password);
    }

    public class LoginService : ILoginService
    {
        // Hard-coded users, to keep it simple.
        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "John", LastName = "Crichton", Username = "john", Password = "secret" },
            new User { Id = 2, FirstName = "Aeryn", LastName = "Sun", Username = "aeryn", Password = "secret" }
        };

        private readonly AppSettings _appSettings;

        public LoginService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // Return null if no user was found.
            if (user == null)
            {
                return null;
            }

            // Authentication successful so generate JWT.
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            // Never return the password.
            user.Password = null;

            return user;
        }
    }
}