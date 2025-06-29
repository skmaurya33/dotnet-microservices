using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiBlog.Context;
using MsRestApiBlog.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsRestApiBlog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class BlogController : ControllerBase
	{
		private readonly AppDbContext _context;
		//private readonly int _userId;

		public BlogController(AppDbContext context)
		{
			_context = context;
			//_userId = 0; // Replace with actual user ID retrieval logic
		}

		// GET: api/<BlogController>
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var rows = await _context.Blogs.ToListAsync();
			return Ok(rows);
		}

		// GET api/<BlogController>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var row = await _context.Blogs.FindAsync(id);
			return Ok(row);
		}

		// POST api/<BlogController>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Blog value)
		{
			int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

			value.UserId = userId;
			value.CreatedAt = DateTime.UtcNow;
			value.UpdatedAt = DateTime.UtcNow;
			_context.Blogs.Add(value);
			await _context.SaveChangesAsync();
			return Ok();
		}

		// PUT api/<BlogController>/5
		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] Blog value)
		{
			var row = await _context.Blogs.FindAsync(id);
			if (row != null)
			{
				row.Title = value.Title;
				row.Description = value.Description;
				row.UpdatedAt = DateTime.UtcNow;
				_context.Blogs.Update(row);
				await _context.SaveChangesAsync();
				return Ok();
			}
			return NotFound();
		}

		// DELETE api/<BlogController>/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var row = await _context.Blogs.FindAsync(id);

			if (row != null)
			{
				_context.Blogs.Remove(row);
				await _context.SaveChangesAsync();
				return Ok();
			}
			return NotFound();
		}
	}
}
