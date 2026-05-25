using Microsoft.EntityFrameworkCore;
using MsRestApiComment.Context;
using Shared.Messages;
using MsRestApiComment.Services;
using NServiceBus;
using Microsoft.OpenApi.Models;

namespace MsRestApiComment
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// ✅ Configure NServiceBus
			builder.Host.UseNServiceBus(context =>
			{
				var endpointConfiguration = new EndpointConfiguration("sbq-dev01-poc-MsRestApiComment");
				
				// Enable auto-creation of queues and topics (for development)
				endpointConfiguration.EnableInstallers();
				
				// Configure Azure Service Bus transport
				var azureServiceBusConnectionString = context.Configuration.GetConnectionString("AzureServiceBus");
				var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
				transport.ConnectionString(azureServiceBusConnectionString);
				
				// ✅ Custom topic name instead of "bundle-1"
				transport.TopicName("sbt-dev01-poc-nservicebus");
				
				// Configure serialization (mandatory in NServiceBus 9.0+)
				endpointConfiguration.UseSerialization<SystemJsonSerializer>();
				
				// Configure error handling
				endpointConfiguration.SendFailedMessagesTo("sbq-dev01-poc-nservicebus-error");
				endpointConfiguration.AuditProcessedMessagesTo("sbq-dev01-poc-nservicebus-audit");
				
				// Configure routing - specify where to send events
				var routing = transport.Routing();
				// Below (sbq-dev01-notification-poc) is the Queue name in service bus
				routing.RouteToEndpoint(typeof(CommentCreatedEvent), "sbq-dev01-poc-MsRestApiAuth");
				
				return endpointConfiguration;
			});

			// ✅ Configure Database
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
			builder.Services.AddDbContext<AppDbContext>(options => 
				options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

			// ✅ Configure Services
			builder.Services.AddHttpClient<IUserService, UserService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddMemoryCache();

			// ✅ Configure Logging
			builder.Services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddConsole();
				loggingBuilder.AddFile("Logs/comment-service-{Date}.txt");
			});

			// ✅ Configure JWT Authentication
			var jwtConfig = builder.Configuration.GetSection("Jwt");
			builder.Services.AddAuthentication("Bearer")
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
						IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
							System.Text.Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
					};
				});

			// ✅ Configure Controllers and Swagger
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Description = "JWT Authorization header using the Bearer scheme. <br><br>Example: \"Bearer xxxxxxxxxxxxxxxxx...\"",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
							Scheme = "oauth2",
							Name = "Bearer",
							In = ParameterLocation.Header,
						},
						new List<string>()
					}
				});
			});

			// ✅ Build the application
			var app = builder.Build();

			// ✅ Configure the HTTP request pipeline
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			// Authentication & Authorization pipeline
			app.UseAuthentication();
			app.UseRouting();
			app.UseAuthorization();

			// Map controllers
			app.MapControllers();

			// ✅ Run the application
			app.Run();
		}
	}
}
