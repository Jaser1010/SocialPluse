using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SocialPluse.Web.Middleware
{
	public class CorrelationIdMiddleware
	{
		private readonly RequestDelegate _next;
		private const string CorrelationIdHeaderName = "X-Correlation-ID";

		public CorrelationIdMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// 1. Check if the incoming request already has a correlation ID (e.g., from a frontend or another microservice)
			// If not, generate a new one.
			string correlationId = GetCorrelationId(context);

			// 2. Add it to the Response headers so the client/frontend can read it
			context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);

			// 3. Push the property into the Serilog LogContext
			// Everything inside this 'using' block will have the "CorrelationId" property automatically attached.
			using (LogContext.PushProperty("CorrelationId", correlationId))
			{
				await _next(context);
			}
		}

		private static string GetCorrelationId(HttpContext context)
		{
			if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues headerValue))
			{
				return headerValue.ToString();
			}
			return Guid.NewGuid().ToString();
		}
	}
}
