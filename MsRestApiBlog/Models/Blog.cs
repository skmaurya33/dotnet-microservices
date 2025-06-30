namespace MsRestApiBlog.Models
{
	public class Blog
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

	// DTO for returning blog data with author information
	public class BlogWithAuthorDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string AuthorName { get; set; }
		public string AuthorEmail { get; set; }
	}

	// DTO for User data from Auth service
	public class UserDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}

	// Paginated response wrapper
	public class PaginatedResponse<T>
	{
		public List<T> Data { get; set; }
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public bool HasNextPage { get; set; }
		public bool HasPreviousPage { get; set; }
	}
}
