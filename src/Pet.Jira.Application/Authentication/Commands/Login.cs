using System;
using System.Security.Authentication;
using MediatR;
using Pet.Jira.Application.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Worklogs.Commands
{
    public class Login
    {
        public class Command : IRequest<Model>
        {
            public Command(LoginRequest request)
            {
                _request = request;
            }

            private readonly LoginRequest _request;
            public LoginRequest Request => _request;
        }

        public class Model
        {
            public LoginResponse Response { get; set; }
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly IAuthenticationService _authenticationService;

            public Handler(IAuthenticationService authenticationService)
            {
                _authenticationService = authenticationService;
            }

            public async Task<Model> Handle(
                Command command,
                CancellationToken cancellationToken)
            {
                try
                {
                    var loginResult = await _authenticationService.LoginAsync(command.Request);
                    return new Model { Response = loginResult };
                }
                catch (AuthenticationException e)
                {
                    throw new Exception($"Authentication exception") { Source = e.Source };
                }
            }
        }
    }
}