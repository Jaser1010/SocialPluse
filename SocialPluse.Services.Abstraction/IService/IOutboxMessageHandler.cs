using System.Text.Json;

namespace SocialPluse.Services.Abstraction.IService
{
	public interface IOutboxMessageHandler
	{
		// Tells the engine which message type this handler is responsible for
		string HandledMessageType { get; }

		// The actual execution logic
		Task HandleAsync(Guid messageId, JsonDocument payload, CancellationToken cancellationToken);
	}
}