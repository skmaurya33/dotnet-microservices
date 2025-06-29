namespace MsRestApiComment.Models
{
	public class Comment
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public int BlogId { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
