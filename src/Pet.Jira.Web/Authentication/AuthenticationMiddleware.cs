using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Pet.Jira.Application.Authentication;

namespace Pet.Jira.Web.Authentication
{
    public class AuthenticationMiddleware
    {
        public static IDictionary<Guid, LoginRequest> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginRequest>();

        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);
                var info = Logins[key];

                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, info.Username),
                    new Claim(ClaimTypes.UserData, info.Password)
                };
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                Logins.Remove(key);
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
