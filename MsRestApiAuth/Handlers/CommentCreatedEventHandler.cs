using MsRestApiAuth.Context;
using MsRestApiAuth.Domain.Entities;
using Shared.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace MsRestApiAuth.Handlers
{
    public class CommentCreatedEventHandler : IHandleMessages<CommentCreatedEvent>
    {
        private readonly AppDbContext _context;
        private static readonly ILog log = LogManager.GetLogger<CommentCreatedEventHandler>();

        public CommentCreatedEventHandler(AppDbContext context)
        {
            _context = context;
            log.Info("CommentCreatedEventHandler constructor called - Handler is registered!");
        }

        public async Task Handle(CommentCreatedEvent message, IMessageHandlerContext context)
        {
            log.Info($"🎉 HANDLER CALLED! Received CommentCreatedEvent for CommentId: {message.CommentId}, UserId: {message.UserId}, BlogId: {message.BlogId}");

            try
            {
				// Create a notification for the comment
				/*var notification = new Notification
                {
                    UserId = message.UserId,
                    Message = $"Your comment '{message.Description}' has been created successfully.",
                    Type = "CommentCreated",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };*/

                log.Info("Creating notification for the comment event");

                var notification = new Notification
                {
					FromUserId = message.UserId,
					ToUserId = message.UserId,
					BlogId = message.BlogId,
					CommentId = message.CommentId,
					Description = $"Your comment on blog {message.BlogId} has been created successfully.",
                    //Type = "CommentCreated",
                    //IsRead = false,
                    CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
                };

                log.Info($"Saving notification to database: FromUserId={notification.FromUserId}, ToUserId={notification.ToUserId}, BlogId={notification.BlogId}, CommentId={notification.CommentId}");

				_context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                log.Info($"✅ Notification created successfully for UserId: {message.UserId}, NotificationId should be auto-generated");
            }
            catch (Exception ex)
            {
                log.Error($"❌ Error processing CommentCreatedEvent: {ex.Message}", ex);
                throw; // Re-throw to let NServiceBus handle retry logic
            }
        }
    }
} 