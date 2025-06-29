using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MsRestApiAuth.Context;
using MsRestApiAuth.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsRestApiAuth.Controllers
{
	[Route("api/auth/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _config;


		public AuthController(AppDbContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] LoginRequestModel model)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

			if (user != null)
			{
				var token = GenerateToken(user);

				return Ok(new { token });
			}
			return BadRequest();
		}

		private string GenerateToken(User user)
		{
			var token = string.Empty;

			var jwtConfig = _config.GetSection("Jwt");

			var claims = new[] {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, user.Name)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!)); // Replace with your secret key
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var tokenObj = new JwtSecurityToken(
				issuer: jwtConfig["Issuer"],
				audience: jwtConfig["Audience"],
				claims: claims,
				expires: DateTime.Now.AddMinutes(Convert.ToInt16(jwtConfig["ExpirationInMinutes"])), // Token expiration time
				signingCredentials: creds
			);

			token = new JwtSecurityTokenHandler().WriteToken(tokenObj);

			return token;
		}
	}
}
