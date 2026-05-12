using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.DbContexts;
using SocialPluse.Services.Abstraction.IService;
using System.Text.Json;

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

				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}
		}

		private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var handlers = scope.ServiceProvider.GetServices<IOutboxMessageHandler>();
			var handlerMap = handlers.ToDictionary(h => h.HandledMessageType);

			var messages = await dbContext.OutboxMessages
				.FromSqlRaw(@"
                    SELECT * FROM ""OutboxMessages""
                    WHERE ""ProcessedOnUtc"" IS NULL
                    ORDER BY ""OccurredOnUtc""
                    FOR UPDATE SKIP LOCKED
                    LIMIT 20")
				.ToListAsync(stoppingToken);

			if (messages.Count == 0) return;

			foreach (var message in messages)
			{
				try
				{
					// 1. THE IDEMPOTENCY CHECK
					// Has this exact message ID already been processed successfully in the past?
					var alreadyProcessed = await dbContext.IdempotentRecords
						.AnyAsync(r => r.Id == message.Id, stoppingToken);

					if (alreadyProcessed)
					{
						// A ghost retry! Mark the outbox as processed and skip execution.
						_logger.LogInformation("Idempotency shield activated. Skipping duplicate message {Id}", message.Id);
						message.ProcessedOnUtc = DateTime.UtcNow;
						continue;
					}

					if (handlerMap.TryGetValue(message.Type, out var handler))
					{
						using var doc = JsonDocument.Parse(message.Content);

						// 2. ATOMIC EXECUTION (Wrap the business logic and the idempotency record together)
						using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

						// Execute the business logic
						await handler.HandleAsync(message.Id, doc, stoppingToken);

						// Add the shield record so this can never run again
						var shieldRecord = IdempotentRecord.Create(message.Id, message.Type);
						dbContext.IdempotentRecords.Add(shieldRecord);

						// Mark the outbox message as complete
						message.ProcessedOnUtc = DateTime.UtcNow;

						// Save everything atomically
						await dbContext.SaveChangesAsync(stoppingToken);
						await transaction.CommitAsync(stoppingToken);
					}
					else
					{
						_logger.LogWarning("No handler registered for Outbox Message Type: {Type}", message.Type);
					}
				}
				catch (Exception ex)
				{
					message.Error = ex.Message;
					_logger.LogError(ex, "Failed to process Outbox Message {Id}", message.Id);

					// We save the error immediately, but DO NOT write to IdempotentRecords.
					// This allows the system to retry it on the next sweep.
					await dbContext.SaveChangesAsync(stoppingToken);
				}
			}
		}
	}
}