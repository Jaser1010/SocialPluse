using System.Net;
using System.Text.Json;

namespace SocialPluse.Web.Middleware
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionMiddleware> _logger;
		private readonly IHostEnvironment _environment;

		public GlobalExceptionMiddleware(RequestDelegate next,ILogger<GlobalExceptionMiddleware> logger,IHostEnvironment environment)
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
				_logger.LogError(ex,
					"Unhandled exception occurred. Path: {Path}, Method: {Method}",
					context.Request.Path,
					context.Request.Method);

				await HandleExceptionAsync(context, ex, _environment);
			}
		}





		private static async Task HandleExceptionAsync(HttpContext context,Exception exception,IHostEnvironment environment)
		{
			context.Response.ContentType = "application/json";

			var statusCode = exception switch
			{
				ArgumentNullException => (int)HttpStatusCode.BadRequest,
				ArgumentException => (int)HttpStatusCode.BadRequest,
				KeyNotFoundException => (int)HttpStatusCode.NotFound,
				UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
				_ => (int)HttpStatusCode.InternalServerError
			};

			context.Response.StatusCode = statusCode;

			var response = new
			{
				success = false,
				message = environment.IsDevelopment()
					? exception.Message
					: "An unexpected error occurred.",
				exceptionType = environment.IsDevelopment()
					? exception.GetType().Name
					: null,
				stackTrace = environment.IsDevelopment()
					? exception.StackTrace
					: null,
				path = context.Request.Path.Value,
				statusCode = statusCode
			};

			var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			await context.Response.WriteAsync(json);
		}



	}
}
