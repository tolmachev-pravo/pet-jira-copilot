using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Pet.Jira.Application.Authentication;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Authentication
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoginStorage _loginStorage;

        public AuthenticationMiddleware(RequestDelegate next, ILoginStorage loginStorage)
        {
            _next = next;
            _loginStorage = loginStorage;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);
                if (!_loginStorage.TryGetValue(key, out var loginDto))
                {
                    context.Response.Redirect("/error");
                    return;
                }

                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, loginDto.Username),
                    new Claim(ClaimTypes.UserData, loginDto.Password)
                };
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddDays(30),
                        IsPersistent = true,
                        AllowRefresh = true
                    });

                _loginStorage.TryRemove(key, out _);
                context.Response.Redirect("/");
                return;

            }
            else if (context.Request.Path == "/logout")
            {
                await context.SignOutAsync();
                context.Response.Redirect("/");
                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
