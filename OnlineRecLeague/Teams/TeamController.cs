﻿using OnlineRecLeague.Users;
using Microsoft.AspNetCore.Mvc;

namespace OnlineRecLeague.Teams
{
	[Route("api/teams")]
	public class TeamController : Controller
	{
		[RequiresLoggedInUser]
		[HttpPost("create")]
		public IActionResult CreateTeam(CreateTeamRequest request)
		{
			return Json(new CreateTeamRequestHandler().CreateTeam(request, UserFromSession));
		}

		private IUser UserFromSession => new UserSessionStore().FindUserOrThrow(HttpContext.Session);
	}
}