using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiBlog.Context;
using MsRestApiBlog.Models;
using MsRestApiBlog.Services;
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
		private readonly IUserService _userService;

		public BlogController(AppDbContext context, IUserService userService)
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

		// GET: api/<BlogController>?page=1&pageSize=10
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			// ✅ Extract JWT token from Authorization header
			var authToken = GetAuthToken();
			if (string.IsNullOrEmpty(authToken))
			{
				return Unauthorized("Authentication token is required");
			}

			// Validate pagination parameters
			if (page < 1) page = 1;
			if (pageSize < 1 || pageSize > 100) pageSize = 10; // Max 100 records per page

			// Get total count for pagination
			var totalCount = await _context.Blogs.CountAsync();

			// Get paginated blogs
			var blogs = await _context.Blogs
				.OrderByDescending(b => b.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Get unique user IDs
			var userIds = blogs.Select(b => b.UserId).Distinct().ToList();

			// ✅ Fetch user data from Auth service with JWT token from request
			var users = await _userService.GetUsersByIdsOptimizedAsync(userIds, authToken);
			var userDict = users.ToDictionary(u => u.Id, u => u);

			// Combine blog data with author information
			var blogsWithAuthors = blogs.Select(blog => new BlogWithAuthorDto
			{
				Id = blog.Id,
				UserId = blog.UserId,
				Title = blog.Title,
				Description = blog.Description,
				CreatedAt = blog.CreatedAt,
				UpdatedAt = blog.UpdatedAt,
				AuthorName = userDict.ContainsKey(blog.UserId) ? userDict[blog.UserId].Name : "Unknown Author",
				AuthorEmail = userDict.ContainsKey(blog.UserId) ? userDict[blog.UserId].Email : ""
			}).ToList();

			// Create paginated response
			var response = new PaginatedResponse<BlogWithAuthorDto>
			{
				Data = blogsWithAuthors,
				CurrentPage = page,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
				HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
				HasPreviousPage = page > 1
			};

			return Ok(response);
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
