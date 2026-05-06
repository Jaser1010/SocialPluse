namespace SocialPluse.Shared.DTOs.Users
{
	public class UserAnalyticsDto
	{
		public int PostsCount { get; set; }
		public int FollowersCount { get; set; }
		public int FollowingCount { get; set; }
		public int LikesReceived { get; set; }
		public int CommentsReceived { get; set; }
		public int BookmarksCount { get; set; }
		public int UnreadNotifications { get; set; }
	}
}
