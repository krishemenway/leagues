﻿using LeagueService.CommonDataTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LeagueService.Users
{
	public class RequiresLoggedInUserAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (new UserSessionStore().FindUser(context.HttpContext.Session) == null)
			{
				context.Result = new JsonResult(Result.Failure("You must be logged in to perform this action"));
			}
		}
	}
}