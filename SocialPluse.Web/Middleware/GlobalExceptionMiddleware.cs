using System.Net;
using System.Text.Json;

namespace SocialPluse.Web.Middleware
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionMiddleware> _logger;
		private readonly IHostEnvironment _environment;

		// PERFORMANCE: Reusing options prevents thousands of allocations under load
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // standard for Frontend/React
			WriteIndented = true
		};

		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
		{
			_next = next;
			_logger = logger;
			_environment = environment;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
				await HandleExceptionAsync(context, ex, _environment);
			}
		}

		private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment environment)
		{
			context.Response.ContentType = "application/json";

			// LOGIC: Map internal exceptions to HTTP status codes
			var statusCode = exception switch
			{
				ArgumentNullException or ArgumentException => (int)HttpStatusCode.BadRequest,
				KeyNotFoundException => (int)HttpStatusCode.NotFound,

				// SENIOR REFINEMENT: Differentiate "Who are you?" (401) from "Not allowed!" (403)
				UnauthorizedAccessException => context.User.Identity?.IsAuthenticated == true
					? (int)HttpStatusCode.Forbidden
					: (int)HttpStatusCode.Unauthorized,

				InvalidOperationException => (int)HttpStatusCode.Conflict, // Perfect for "Already Liked" or "Business Rule" errors

				_ => (int)HttpStatusCode.InternalServerError
			};

			context.Response.StatusCode = statusCode;

			// SECURITY: Mask sensitive system errors in Production, but show helpful client errors
			// UPDATE: Safely extracting the InnerException to expose database errors in Development
			var message = (statusCode == 500 && !environment.IsDevelopment())
				? "An unexpected server error occurred."
				: exception.InnerException != null
					? $"{exception.Message} | Inner Details: {exception.InnerException.Message}"
					: exception.Message;

			var response = new
			{
				success = false,
				message,
				exceptionType = environment.IsDevelopment() ? exception.GetType().Name : null,
				stackTrace = environment.IsDevelopment() ? exception.StackTrace : null,
				path = context.Request.Path.Value,
				statusCode
			};

			var json = JsonSerializer.Serialize(response, JsonOptions);
			await context.Response.WriteAsync(json);
		}
	}
}