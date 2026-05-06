using SocialPluse.Domain.Entities;
using SocialPluse.Persistence.IdentityData.Entities;
using SocialPluse.Shared.DTOs.Comments;
using SocialPluse.Shared.DTOs.Follows;
using SocialPluse.Shared.DTOs.Likes;
using SocialPluse.Shared.DTOs.Notifications;
using SocialPluse.Shared.DTOs.Posts;
using SocialPluse.Shared.DTOs.Safety;
using SocialPluse.Shared.DTOs.Users;

namespace SocialPluse.Services.Mappers
{
	public static class EntityMappers
	{
		// 1. Notification Mapper (Handles the tricky Enum cast!)
		public static NotificationDto ToDto(this Notification entity, string actorUsername)
		{
			return new NotificationDto
			{
				Id = entity.Id,
				ActorUserId = entity.ActorUserId,
				ActorUsername = actorUsername,
				Type = (SocialPluse.Shared.Enums.NotificationType)entity.Type, // Explicit cast hidden here
				PostId = entity.PostId,
				CommentId = entity.CommentId,
				IsRead = entity.IsRead,
				CreatedAt = entity.CreatedAt
			};
		}

		// 2. Comment Mapper
		public static CommentDto ToDto(this Comment entity, string authorUsername)
		{
			return new CommentDto
			{
				Id = entity.Id,
				PostId = entity.PostId,
				AuthorId = entity.AuthorId,
				AuthorUsername = authorUsername,
				Text = entity.Text,
				CreatedAt = entity.CreatedAt
			};
		}

		// 3. Post Mapper
		public static PostDto ToDto(this Post entity, string authorUsername, int likesCount, int commentsCount, bool isLiked, bool isBookmarked)
		{
			return new PostDto
			{
				Id = entity.Id,
				AuthorId = entity.AuthorId,
				AuthorUsername = authorUsername,
				Text = entity.Text,
				MediaUrl = entity.MediaUrl,
				LikesCount = likesCount,
				CommentsCount = commentsCount,
				IsLikedByCurrentUser = isLiked,
				IsBookmarkedByCurrentUser = isBookmarked,
				CreatedAt = entity.CreatedAt
			};
		}

		// 4. Follow Mapper
		public static FollowResponse ToResponse(this Follow entity)
		{
			return new FollowResponse
			{
				FollowerId = entity.FollowerId,
				FolloweeId = entity.FolloweeId,
				CreatedAt = entity.CreatedAt
			};
		}

		// 5. Like Mapper
		public static LikeResponse ToResponse(this Like entity)
		{
			return new LikeResponse
			{
				UserId = entity.UserId,
				PostId = entity.PostId,
				CreatedAt = entity.CreatedAt
			};
		}


		
		public static BlockResponse ToResponse(this Block entity)
		{
			return new BlockResponse { BlockerId = entity.BlockerId, BlockedId = entity.BlockedId, CreatedAt = entity.CreatedAt };
		}

		public static MuteResponse ToResponse(this Mute entity)
		{
			return new MuteResponse { MuterId = entity.MuterId, MutedId = entity.MutedId, CreatedAt = entity.CreatedAt };
		}

		public static ReportDto ToDto(this Report entity)
		{
			return new ReportDto
			{
				Id = entity.Id,
				ReporterId = entity.ReporterId,
				TargetType = entity.TargetType,
				TargetId = entity.TargetId,
				Reason = entity.Reason,
				Status = entity.Status,
				CreatedAt = entity.CreatedAt
			};
		}

		public static UserProfileDto ToProfileDto(this AppUser entity)
		{
			return new UserProfileDto
			{
				Id = entity.Id.ToString(),
				Username = entity.UserName!,
				Email = entity.Email!,
				DisplayName = entity.DisplayName,
				Bio = entity.Bio,
				AvatarUrl = entity.AvatarUrl,
				CreatedAt = entity.CreatedAt
			};
		}


	}
}