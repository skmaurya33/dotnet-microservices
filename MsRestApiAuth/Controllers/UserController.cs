using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiAuth.Context;
using MsRestApiAuth.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsRestApiAuth.Controllers
{
	[Route("api/auth/[controller]")]
	[ApiController]
	[Authorize] 
	public class UserController : ControllerBase
	{
		private readonly AppDbContext _context;
		public UserController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/<UserController>
		[HttpGet]
		public async Task<IActionResult> Get(CancellationToken cancellationToken)
		{
			var list = await _context.Users.ToListAsync();
			return Ok(list);
		}

		// GET api/<UserController>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var user = await _context.Users.FindAsync(id);
			return Ok(user);
		}

		// POST api/<UserController>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] User value)
		{
			value.Id = 0;
			value.CreatedAt = DateTime.UtcNow;
			value.UpdatedAt = DateTime.UtcNow;

			_context.Users.Add(value);
			await _context.SaveChangesAsync();
			return Ok();
		}

		// PUT api/<UserController>/5
		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] User value)
		{
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

		// DELETE api/<UserController>/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user != null)
			{
				_context.Users.Remove(user);
				await _context.SaveChangesAsync();
				return Ok();
			}
			else
			{
				return NotFound();
			}
		}
	}
}
