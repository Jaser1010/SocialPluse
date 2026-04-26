using SocialPluse.Shared.DTOs.Follows;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPluse.Services.Abstraction
{
	public interface IFollowService
	{
		Task<FollowResponse> FollowAsync(Guid followerId, Guid followeeId);
		Task UnfollowAsync(Guid followerId, Guid followeeId);
		Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
	}
}
