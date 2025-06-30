using System.ComponentModel.DataAnnotations;

namespace MsRestApiAuth.Domain.Models
{
	public class LoginRequestModel
	{
		[Required]
		public required string Email { get; set; }

		[Required]
		public required string Password { get; set; }
	}
}
