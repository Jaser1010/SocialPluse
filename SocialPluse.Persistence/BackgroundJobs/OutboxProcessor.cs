using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IService;

namespace SocialPluse.Persistence.BackgroundJobs
{
	public class OutboxProcessor : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<OutboxProcessor> _logger;

		public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessOutboxMessagesAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Critical failure in OutboxProcessor sweep.");
				}

				// Poll the database every 10 seconds. 
				// In massive enterprise systems, this can be swapped for a Quartz Cron Job.
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}
		}

		private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
		{
			// Background services are Singletons. To use Scoped services like AppDbContext, 
			// we MUST create a temporary scope for this sweep.
			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			// 1. Fetch unprocessed messages (Batching 20 at a time prevents memory spikes)
			var messages = await dbContext.OutboxMessages
				.Where(m => m.ProcessedOnUtc == null)
				.OrderBy(m => m.OccurredOnUtc)
				.Take(20)
				.ToListAsync(stoppingToken);

			if (messages.Count == 0) return;

			// Resolve target domain services
			var feedService = scope.ServiceProvider.GetRequiredService<IFeedService>();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

			foreach (var message in messages)
			{
				try
				{
					var doc = JsonDocument.Parse(message.Content);

					// 2. Route the message to the exact service logic
					switch (message.Type)
					{
						case "FanoutPostToFeed":
							await feedService.FanoutPostToFeedAsync(
								doc.RootElement.GetProperty("PostId").GetGuid(),
								doc.RootElement.GetProperty("AuthorId").GetGuid());
							break;

						case "CreateCommentNotification":
							await notificationService.CreateCommentNotificationAsync(
								doc.RootElement.GetProperty("PostAuthorId").GetGuid(),
								doc.RootElement.GetProperty("CommentAuthorId").GetGuid(),
								doc.RootElement.GetProperty("PostId").GetGuid(),
								doc.RootElement.GetProperty("CommentId").GetGuid());
							break;

						case "CreateLikeNotification":
							await notificationService.CreateLikeNotificationAsync(
								doc.RootElement.GetProperty("RecipientId").GetGuid(),
								doc.RootElement.GetProperty("ActorId").GetGuid(),
								doc.RootElement.GetProperty("PostId").GetGuid());
							break;

						case "CreateFollowNotification":
							await notificationService.CreateFollowNotificationAsync(
								doc.RootElement.GetProperty("RecipientId").GetGuid(),
								doc.RootElement.GetProperty("ActorId").GetGuid());
							break;

						case "BackfillFolloweeFeed":
							await feedService.BackfillFolloweeFeedAsync(
								doc.RootElement.GetProperty("FollowerId").GetGuid(),
								doc.RootElement.GetProperty("FolloweeId").GetGuid());
							break;

						case "InvalidateFeedCache":
							await feedService.InvalidateFeedCacheAsync(
								doc.RootElement.GetProperty("UserId").GetGuid());
							break;

						case "CreateReportNotification":
							await notificationService.CreateReportNotificationAsync(
								doc.RootElement.GetProperty("TargetId").GetGuid(),
								doc.RootElement.GetProperty("ReporterId").GetGuid());
							break;

						default:
							_logger.LogWarning("Unknown Outbox Message Type: {Type}", message.Type);
							break;
					}

					// 3. Mark as successfully processed
					message.ProcessedOnUtc = DateTime.UtcNow;
				}
				catch (Exception ex)
				{
					// 4. Capture the error, but DO NOT crash the loop. 
					// ProcessedOnUtc remains null, so it will retry on the next sweep.
					message.Error = ex.Message;
					_logger.LogError(ex, "Failed to process Outbox Message {Id}", message.Id);
				}
			}

			// 5. Commit the updates (marking them as processed) back to the database
			await dbContext.SaveChangesAsync(stoppingToken);
		}
	}
}