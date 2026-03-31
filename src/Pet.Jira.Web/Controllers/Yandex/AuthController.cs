using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pet.Jira.Application.Integrations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Controllers.Yandex
{
	[Authorize]
	[ApiController]
	[Route("profile/integrations/yandex")]
	public class AuthController : ControllerBase
	{
		private readonly IUserCalendarConnectionService _calendarConnectionService;

		public AuthController(IUserCalendarConnectionService calendarConnectionService)
		{
			_calendarConnectionService = calendarConnectionService;
		}

		[HttpGet("connect")]
		public async Task<IActionResult> Connect(CancellationToken cancellationToken)
		{
			try
			{
				var url = await _calendarConnectionService.BuildConnectUrlAsync(cancellationToken);
				return Redirect(url);
			}
			catch
			{
				return Redirect("/profile?yandexStatus=error");
			}
		}

		[HttpGet("callback")]
		public async Task<IActionResult> Callback(
			[FromQuery] string code,
			[FromQuery] string state,
			[FromQuery] string error,
			[FromQuery(Name = "error_description")] string errorDescription,
			CancellationToken cancellationToken)
		{
			try
			{
				var result = await _calendarConnectionService.HandleCallbackAsync(
					code,
					state,
					error,
					errorDescription,
					cancellationToken);

				var status = result.IsSuccess ? "connected" : "error";
				return Redirect($"/profile?yandexStatus={status}");
			}
			catch
			{
				return Redirect("/profile?yandexStatus=error");
			}
		}

		[HttpPost("disconnect")]
		public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
		{
			try
			{
				await _calendarConnectionService.DisconnectAsync(cancellationToken);
				return Redirect("/profile?yandexStatus=disconnected");
			}
			catch
			{
				return Redirect("/profile?yandexStatus=error");
			}
		}
	}
}
