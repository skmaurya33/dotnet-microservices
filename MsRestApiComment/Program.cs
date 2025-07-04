using Microsoft.EntityFrameworkCore;
using MsRestApiComment.Context;
using Shared.Messages;
using MsRestApiComment.Services;
using NServiceBus;

namespace MsRestApiComment
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseNServiceBus(context =>
				{
					var endpointConfiguration = new EndpointConfiguration("MsRestApiComment");
					
					// ✅ Enable auto-creation of queues and topics (for development)
					endpointConfiguration.EnableInstallers();
					
					// Configure Azure Service Bus transport
					var azureServiceBusConnectionString = context.Configuration.GetConnectionString("AzureServiceBus");
					var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
					transport.ConnectionString(azureServiceBusConnectionString);
					
					// ✅ Configure serialization (mandatory in NServiceBus 9.0+)
					endpointConfiguration.UseSerialization<SystemJsonSerializer>();
					
					// Configure error handling
					endpointConfiguration.SendFailedMessagesTo("error");
					endpointConfiguration.AuditProcessedMessagesTo("audit");
					
					// Configure routing - specify where to send events
					var routing = transport.Routing();
					routing.RouteToEndpoint(typeof(Shared.Messages.CommentCreatedEvent), "MsRestApiAuth");
					
					return endpointConfiguration;
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}

	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			var connectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

			// Register HttpClient and Services
			services.AddHttpClient<IUserService, UserService>();
			services.AddScoped<IUserService, UserService>();
			
			// ✅ Add Memory Cache for user data caching
			services.AddMemoryCache();

			// ✅ Add file logging
			services.AddLogging(builder =>
			{
				builder.AddConsole(); // Keep console logging
				builder.AddFile("Logs/comment-service-{Date}.txt"); // Add file logging
			});

			var jwtConfig = Configuration.GetSection("Jwt");

			services.AddAuthentication("Bearer")
				.AddJwtBearer("Bearer", options =>
				{
					options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = jwtConfig["Issuer"],
						ValidAudience = jwtConfig["Audience"],
						IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
					};
				});

			services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(c =>
			{
				c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Description = "JWT Authorization header using the Bearer scheme. <br><br>Example: \"Bearer xxxxxxxxxxxxxxxxx...\"",
					Name = "Authorization",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});

				c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
				{
					{
						new Microsoft.OpenApi.Models.OpenApiSecurityScheme
						{
							Reference = new Microsoft.OpenApi.Models.OpenApiReference
							{
								Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
							Scheme = "oauth2",
							Name = "Bearer",
							In = Microsoft.OpenApi.Models.ParameterLocation.Header,
						},
						new List<string>()
					}
				});
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			// Configure the HTTP request pipeline.
			if (env.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			// ✅ 1. Authentication validates JWT tokens
			app.UseAuthentication();
			
			// ✅ 2. Routing determines which endpoint to call
			app.UseRouting();
			
			// ✅ 3. Authorization checks permissions (MUST be between UseRouting and UseEndpoints)
			app.UseAuthorization();

			// ✅ 4. Execute the endpoints
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
