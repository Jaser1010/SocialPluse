namespace SocialPluse.Domain.Entities
{
	public class IdempotentRecord
	{
		// This will strictly match the OutboxMessage.Id
		public Guid Id { get; private set; }
		public string MessageType { get; private set; } = string.Empty;
		public DateTime ProcessedOnUtc { get; private set; }

		protected IdempotentRecord() { } // For EF Core

		public static IdempotentRecord Create(Guid messageId, string messageType)
		{
			return new IdempotentRecord
			{
				Id = messageId,
				MessageType = messageType,
				ProcessedOnUtc = DateTime.UtcNow
			};
		}
	}
}