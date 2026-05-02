using System;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Newtonsoft.Json;

namespace Hangfire.Community.Dashboard.Forms.Support
{
	internal class CommandWithResponseDispatcher : IDashboardDispatcher
	{
		private readonly Func<DashboardContext, bool> _command;

		public CommandWithResponseDispatcher(Func<DashboardContext, bool> command)
		{
			this._command = command;
		}

		public Task Dispatch(DashboardContext context)
		{
			DashboardRequest request = context.Request;
			DashboardResponse response = context.Response;
			if (!"POST".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
			{
				response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
				return Task.FromResult(false);
			}

			try
			{
				this._command(context);
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				response.ContentType = "application/json";
				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				return response.WriteAsync(JsonConvert.SerializeObject(new {
					errorMessage = ex.Message,
					exceptionTitle = ex.GetType().Name,
					exceptionMessage = ex.Message,
					stackTrace = ex.ToString()
				}));
			}
		}
	}
}
