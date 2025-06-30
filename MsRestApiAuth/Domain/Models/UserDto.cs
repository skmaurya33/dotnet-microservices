namespace MsRestApiAuth.Domain.Models
{
	// DTO for external API calls (without sensitive data)
	public class UserDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
