using NServiceBus;

namespace Shared.Messages
{
    public class CommentCreatedEvent : IEvent
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public int BlogId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
} 