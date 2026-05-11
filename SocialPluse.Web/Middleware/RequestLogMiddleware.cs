using System.Diagnostics;

namespace SocialPluse.Web.Middleware
{
	public class RequestLogMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLogMiddleware> _logger;

		public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var timer = Stopwatch.StartNew();

			// Let the request proceed through the pipeline
			await _next(context);

			timer.Stop();

			var elapsedMs = timer.Elapsed.TotalMilliseconds;
			var method = context.Request.Method;
			var path = context.Request.Path;
			var statusCode = context.Response.StatusCode;

			// Master-Level Logging: Highlight slow requests (over 200ms)
			if (elapsedMs > 200)
			{
				_logger.LogWarning("PERF ALERT: {Method} {Path} took {Elapsed:N2}ms (Status: {StatusCode})",
					method, path, elapsedMs, statusCode);
			}
			else
			{
				_logger.LogInformation("{Method} {Path} completed in {Elapsed:N2}ms (Status: {StatusCode})",
					method, path, elapsedMs, statusCode);
			}
		}
	}
}