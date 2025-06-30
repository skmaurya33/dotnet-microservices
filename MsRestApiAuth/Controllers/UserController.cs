using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiAuth.Context;
using MsRestApiAuth.Domain.Entities;
using MsRestApiAuth.Domain.Models;

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
			if (user == null)
				return NotFound();

			// Return DTO without password
			var userDto = new UserDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt
			};
			return Ok(userDto);
		}

		// GET api/<UserController>/service/{id} - Secure endpoint for inter-service communication
		[HttpGet("service/{id}")]
		[Authorize]
		public async Task<IActionResult> GetForService(int id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
				return NotFound();

			// Return DTO without password
			var userDto = new UserDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt
			};
			return Ok(userDto);
		}

		// POST api/<UserController>/service/batch - Secure batch endpoint for inter-service communication
		[HttpPost("service/batch")]
		[Authorize]
		public async Task<IActionResult> GetUsersBatch([FromBody] List<int> userIds)
		{
			if (userIds == null || !userIds.Any())
				return BadRequest("User IDs list cannot be empty");

			if (userIds.Count > 100) // Limit batch size
				return BadRequest("Maximum 100 user IDs allowed per batch request");

			var users = await _context.Users
				.Where(u => userIds.Contains(u.Id))
				.Select(u => new UserDto
				{
					Id = u.Id,
					Name = u.Name,
					Email = u.Email,
					CreatedAt = u.CreatedAt,
					UpdatedAt = u.UpdatedAt
				})
				.ToListAsync();

			return Ok(users);
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
