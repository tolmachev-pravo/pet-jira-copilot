using System;

namespace Pet.Jira.Application.Authentication.Dto
{
    public class LoginDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public static LoginDto Create(LoginRequest request)
        {
            return new LoginDto
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Password = request.Password
            };
        }
    }
}
