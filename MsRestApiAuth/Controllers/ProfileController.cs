using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsRestApiAuth.Context;
using MsRestApiAuth.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsRestApiAuth.Controllers
{
	[Route("api/auth/[controller]")]
	[ApiController]
	[Authorize]
	public class ProfileController : ControllerBase
	{
		private readonly AppDbContext _context;
		public ProfileController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/<UserController>
		[HttpGet]
		public async Task<IActionResult> Get(CancellationToken cancellationToken)
		{
			int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
			//var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			//var email = User.FindFirstValue(ClaimTypes.Email);
			//var name = User.FindFirstValue(ClaimTypes.Name);

			var user = await _context.Users.FindAsync(id);

			return Ok(user);
		}

		// PUT api/<UserController>/5
		[HttpPut]
		public async Task<IActionResult> Put([FromBody] User value)
		{
			int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

			var user = await _context.Users.FindAsync(id);
			if (user != null)
			{
				user.Name = value.Name;
				user.Email = value.Email;
				user.Password = value.Password;
				user.UpdatedAt = DateTime.UtcNow;
				_context.Users.Update(user);
				await _context.SaveChangesAsync();
			}
			return Ok();
		}
	}
}
