namespace MsRestApiAuth.Domain.Entities
{
	public class Notification
	{
		public int Id { get; set; }
		public int FromUserId { get; set; }
		public int ToUserId { get; set; }
		public int BlogId { get; set; }
		public int CommentId { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
