using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiComment.Context;
using MsRestApiComment.Models;
using MsRestApiComment.Services;
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
		private readonly IUserService _userService;

		public CommentController(AppDbContext context, IUserService userService)
		{
			_context = context;
			_userService = userService;
		}

		// Helper method to extract JWT token from Authorization header
		private string? GetAuthToken()
		{
			var authHeader = Request.Headers["Authorization"].FirstOrDefault();
			if (authHeader != null && authHeader.StartsWith("Bearer "))
			{
				return authHeader["Bearer ".Length..].Trim();
			}
			return null;
		}

		// GET: api/<CommentController>
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] int blogId = 0)
		{
			// ✅ Extract JWT token from Authorization header
			var authToken = GetAuthToken();
			if (string.IsNullOrEmpty(authToken))
			{
				return Unauthorized("Authentication token is required");
			}

			var comments = await _context.Comments.Where(c => c.BlogId == blogId).ToListAsync();
			
			// Get unique user IDs
			var userIds = comments.Select(c => c.UserId).Distinct().ToList();

			// ✅ Fetch user data from Auth service with JWT token from request
			var users = await _userService.GetUsersByIdsOptimizedAsync(userIds, authToken);
			var userDict = users.ToDictionary(u => u.Id, u => u);

			// Combine comment data with author information
			var commentsWithAuthors = comments.Select(comment => new CommentWithAuthorDto
			{
				Id = comment.Id,
				UserId = comment.UserId,
				BlogId = comment.BlogId,
				Description = comment.Description,
				CreatedAt = comment.CreatedAt,
				UpdatedAt = comment.UpdatedAt,
				AuthorName = userDict.ContainsKey(comment.UserId) ? userDict[comment.UserId].Name : "Unknown Author",
				AuthorEmail = userDict.ContainsKey(comment.UserId) ? userDict[comment.UserId].Email : ""
			}).ToList();

			return Ok(commentsWithAuthors);
		}

		// GET api/<CommentController>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			// ✅ Extract JWT token from Authorization header
			var authToken = GetAuthToken();
			if (string.IsNullOrEmpty(authToken))
			{
				return Unauthorized("Authentication token is required");
			}

			var comment = await _context.Comments.FindAsync(id);
			if (comment == null)
			{
				return NotFound();
			}

			// ✅ Fetch user data for the comment author
			var user = await _userService.GetUserByIdAsync(comment.UserId, authToken);

			// Create comment with author information
			var commentWithAuthor = new CommentWithAuthorDto
			{
				Id = comment.Id,
				UserId = comment.UserId,
				BlogId = comment.BlogId,
				Description = comment.Description,
				CreatedAt = comment.CreatedAt,
				UpdatedAt = comment.UpdatedAt,
				AuthorName = user?.Name ?? "Unknown Author",
				AuthorEmail = user?.Email ?? ""
			};

			return Ok(commentWithAuthor);
		}

		// POST api/<CommentController>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Comment value)
		{
			int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

			value.UserId = userId;
			value.CreatedAt = DateTime.UtcNow;
			value.UpdatedAt = DateTime.UtcNow; // Fixed: was CreatedAt twice

			_context.Comments.Add(value);

			// Write the service bus event logic to notify the auth api to update the notification table
			/*
			 
			 
			 */

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
