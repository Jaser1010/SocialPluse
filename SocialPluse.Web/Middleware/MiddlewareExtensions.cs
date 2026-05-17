namespace SocialPluse.Web.Middleware
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
		{
			return app.UseMiddleware<GlobalExceptionMiddleware>();
		}
		public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CorrelationIdMiddleware>();
		}
	}
}
