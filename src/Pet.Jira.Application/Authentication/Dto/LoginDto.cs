using System;
using Pet.Jira.Domain.Models.Abstract;

namespace Pet.Jira.Application.Authentication.Dto
{
    public class LoginDto : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid Key => Id;

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
