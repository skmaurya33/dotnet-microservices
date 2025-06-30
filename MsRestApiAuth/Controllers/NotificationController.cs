using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsRestApiAuth.Context;
using MsRestApiAuth.Domain.Entities;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsRestApiAuth.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class NotificationController : ControllerBase
	{
		private readonly AppDbContext _context;

		public NotificationController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/<NotificationController>
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var rows = await _context.Notifications.ToListAsync();
			return Ok(rows);
		}

		// GET api/<NotificationController>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var row = await _context.Notifications.FindAsync(id);
			return Ok(row);
		}

		// POST api/<NotificationController>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Notification value)
		{
			_context.Notifications.Add(value);
			await _context.SaveChangesAsync();
			return Ok();
		}
	}
}
