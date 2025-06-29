using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiComment.Context;
using MsRestApiComment.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsRestApiComment.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CommentController : ControllerBase
	{
		private readonly AppDbContext _context;
		//private readonly int _userId;

		public CommentController(AppDbContext context)
		{
			_context = context;
			//_userId = 0;
		}

		// GET: api/<CommentController>
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var rows = await _context.Comments.ToListAsync();
			return Ok(rows);
		}

		// GET api/<CommentController>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var row = await _context.Comments.FindAsync(id);
			return Ok(row);
		}

		// POST api/<CommentController>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Comment value)
		{
			int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

			value.UserId = userId;
			value.CreatedAt = DateTime.UtcNow;
			value.CreatedAt = DateTime.UtcNow;

			_context.Comments.Add(value);
			await _context.SaveChangesAsync();
			return Ok();
		}

		// PUT api/<CommentController>/5
		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] Comment value)
		{
			var row = await _context.Comments.FindAsync(id);

			if (row != null)
			{
				row.Description = value.Description;
				row.UpdatedAt = DateTime.UtcNow;
				_context.Comments.Update(row);
				await _context.SaveChangesAsync();
				return Ok();
			}
			return NotFound();
		}

		// DELETE api/<CommentController>/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var row = _context.Comments.Find(id);
			if (row != null)
			{
				_context.Comments.Remove(row);
				await _context.SaveChangesAsync();
				return Ok();
			}
			return NotFound();
		}
	}
}
