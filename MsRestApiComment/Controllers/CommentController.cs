using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiComment.Context;
using MsRestApiComment.Models;
using MsRestApiComment.Services;
using Shared.Messages;
using NServiceBus;
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
		private readonly IMessageSession _messageSession;
		private readonly ILogger<CommentController> _logger;

		public CommentController(AppDbContext context, IUserService userService, IMessageSession messageSession, ILogger<CommentController> logger)
		{
			_context = context;
			_userService = userService;
			_messageSession = messageSession;
			_logger = logger;
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
			_logger.LogInformation("Starting comment creation process");

			int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
			_logger.LogInformation($"Creating comment for UserId: {userId}, BlogId: {value.BlogId}");

			value.UserId = userId;
			value.CreatedAt = DateTime.UtcNow;
			value.UpdatedAt = DateTime.UtcNow; // Fixed: was CreatedAt twice

			_context.Comments.Add(value);
			await _context.SaveChangesAsync();

			// ✅ Method 1: Direct access to the ID (auto-populated by EF)
			var newCommentId = value.Id; // This is your newly generated PkId!
			_logger.LogInformation($"Comment saved successfully with ID: {newCommentId}");

			// ✅ Alternative methods:
			// Method 2: Query back the entity (if you need fresh data)
			// var savedComment = await _context.Comments.FindAsync(value.Id);
			
			// Method 3: Use the entity tracking to get the ID
			// var entityEntry = _context.Entry(value);
			// var idValue = entityEntry.Property(nameof(Comment.Id)).CurrentValue;

			try
			{
				// ✅ Publish CommentCreatedEvent using NServiceBus
				var commentCreatedEvent = new CommentCreatedEvent
				{
					CommentId = newCommentId, // Use the newly generated ID
					UserId = value.UserId,
					BlogId = value.BlogId,
					Description = value.Description,
					CreatedAt = value.CreatedAt
				};

				_logger.LogInformation($"Publishing CommentCreatedEvent: CommentId={commentCreatedEvent.CommentId}, UserId={commentCreatedEvent.UserId}, BlogId={commentCreatedEvent.BlogId}");

				await _messageSession.Publish(commentCreatedEvent);

				_logger.LogInformation("CommentCreatedEvent published successfully!");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to publish CommentCreatedEvent for CommentId: {newCommentId}");
				// Don't fail the request if event publishing fails
			}

			// ✅ Return the newly created comment with its ID
			return Ok(new { 
				Id = newCommentId,
				Message = "Comment created successfully",
				CommentId = newCommentId 
			});
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

		// ✅ Test endpoint to verify Azure Service Bus connection
		[HttpPost("test-event")]
		public async Task<IActionResult> TestEvent()
		{
			_logger.LogInformation("Testing Azure Service Bus event publishing...");

			try
			{
				var testEvent = new CommentCreatedEvent
				{
					CommentId = 999,
					UserId = 1,
					BlogId = 1,
					Description = "Test event from API",
					CreatedAt = DateTime.UtcNow
				};

				_logger.LogInformation($"Publishing test event: CommentId={testEvent.CommentId}");

				await _messageSession.Publish(testEvent);

				_logger.LogInformation("✅ Test event published successfully!");

				return Ok(new { 
					Message = "Test event published successfully",
					TestEvent = testEvent 
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Failed to publish test event");
				return StatusCode(500, new { 
					Message = "Failed to publish test event",
					Error = ex.Message 
				});
			}
		}
	}
}
