using Blazored.LocalStorage;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Pet.Jira.Application;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Integrations;
using Pet.Jira.Infrastructure;
using Pet.Jira.Infrastructure.Data.Contexts;
using Pet.Jira.Infrastructure.Mock;
using Pet.Jira.Web.Authentication;
using Pet.Jira.Web.Common;
using Pet.Jira.Web.Components.Clipboard;
using Pet.Jira.Web.Components.Markdown;
using Pet.Jira.Web.Logging;
using System;
using Thinktecture.Blazor.AsyncClipboard;

namespace Pet.Jira.Web
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient();
            services.AddDataProtection();

			services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
            });
            services.AddMudMarkdownServices();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IMarkdownService, MarkdownService>();

            services.Configure<YandexOAuthConfiguration>(Configuration.GetSection("YandexOAuth"));

            services.AddInfrastructureLayer(Configuration.GetSection("Jira"));
            services.AddApplicationLayer();
            if (EnvironmentExtensions.IsMock())
            {
                services.AddMockInfrastructureLayer();
            }

            services.AddHttpContextAccessor();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                });

            services.AddBlazoredLocalStorage();

            services.AddAsyncClipboardService();
            services.AddTransient<IClipboard, Clipboard>();

            services.AddHealthChecks()
                .AddInfrastructureHealthChecks();
            services
                .AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddHealthCheckEndpoint("Application health checks", "/health");
                    setup.SetEvaluationTimeInSeconds(30);
                    setup.SetApiMaxActiveRequests(1);
                    setup.SetMinimumSecondsBetweenFailureNotifications(120);
                }).AddInMemoryStorage();

			services.AddControllers();
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();

            app.UseMiddleware<AuthenticationMiddleware>();
            app.UseMiddleware<LogEnrichmentMiddleware>();

			app.UseSwagger();
			app.UseSwaggerUI();

			using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
			}

			app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(setup =>
                {
                    setup.AddCustomStylesheet("wwwroot/css/dotnet.css");
                });
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
			});


		}
    }
}
