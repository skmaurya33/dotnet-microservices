namespace MsRestApiComment.Models
{
    public class CommentWithAuthorDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BlogId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Author information
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
    }
} 