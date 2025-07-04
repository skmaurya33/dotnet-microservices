# Azure Service Bus with NServiceBus Implementation Guide

## Overview
This guide explains how to use Azure Service Bus with NServiceBus in your microservices architecture to enable asynchronous communication between services.

## Architecture Flow

```
Comment Service → [Azure Service Bus] → Auth Service
   (Publisher)                           (Subscriber)
```

When a comment is created:
1. `CommentController` publishes `CommentCreatedEvent` to Azure Service Bus
2. Azure Service Bus delivers the event to `AuthService`
3. `CommentCreatedEventHandler` processes the event and creates a notification

## Prerequisites

### 1. Azure Service Bus Namespace
Create an Azure Service Bus namespace in your Azure subscription:

```bash
# Using Azure CLI
az servicebus namespace create \
    --resource-group myResourceGroup \
    --name myServiceBusNamespace \
    --location eastus \
    --sku Standard
```

### 2. Connection String
Get your connection string from Azure Portal:
- Navigate to your Service Bus namespace
- Go to "Shared access policies"
- Copy the "Primary Connection String"

## Configuration

### 1. Update Connection Strings
Replace the placeholder in `appsettings.json` files:

```json
{
  "ConnectionStrings": {
    "AzureServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  }
}
```

### 2. Development Configuration
For local development, you can use `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AzureServiceBus": "Endpoint=sb://localhost:7000/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-dev-key"
  }
}
```

## Package Dependencies

The following NuGet packages are required:

```xml
<PackageReference Include="NServiceBus" Version="9.2.3" />
<PackageReference Include="NServiceBus.Transport.AzureServiceBus" Version="4.0.0" />
<PackageReference Include="NServiceBus.Extensions.Hosting" Version="3.0.1" />
```

**Note**: NServiceBus 9.0+ uses the built-in `SystemJsonSerializer` and no longer requires a separate JSON serialization package.

## Key Components

### 1. Message Contracts
Located in `Messages/` folder in each service:

```csharp
public class CommentCreatedEvent : IEvent
{
    public int CommentId { get; set; }
    public int UserId { get; set; }
    public int BlogId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 2. Message Publisher (Comment Service)
In `CommentController.cs`:

```csharp
// Publish event
var commentCreatedEvent = new CommentCreatedEvent
{
    CommentId = value.Id,
    UserId = value.UserId,
    BlogId = value.BlogId,
    Description = value.Description,
    CreatedAt = value.CreatedAt
};

await _messageSession.Publish(commentCreatedEvent);
```

### 3. Message Handler (Auth Service)
In `Handlers/CommentCreatedEventHandler.cs`:

```csharp
public class CommentCreatedEventHandler : IHandleMessages<CommentCreatedEvent>
{
    public async Task Handle(CommentCreatedEvent message, IMessageHandlerContext context)
    {
        // Create notification
        var notification = new Notification
        {
            UserId = message.UserId,
            Message = $"Your comment '{message.Description}' has been created successfully.",
            Type = "CommentCreated",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}
```

## Best Practices

### 1. Message Versioning
- Use semantic versioning for message contracts
- Keep old versions for backward compatibility
- Use message polymorphism for upgrades

### 2. Error Handling
- Configure retry policies
- Use dead letter queues for failed messages
- Implement circuit breakers for external dependencies

### 3. Performance
- Use message batching for high-volume scenarios
- Configure prefetch counts appropriately
- Monitor queue lengths and processing times

### 4. Security
- Use managed identity in production
- Rotate connection strings regularly
- Apply principle of least privilege

## Running the Application

### 1. Start Services
```bash
# Start Comment Service
cd MsRestApiComment
dotnet run

# Start Auth Service (in separate terminal)
cd MsRestApiAuth
dotnet run
```

### 2. Test the Flow
1. Create a comment via POST to `/api/Comment`
2. Check the Auth service logs for message processing
3. Verify notification creation in the database

## Monitoring and Troubleshooting

### 1. Service Bus Metrics
Monitor in Azure Portal:
- Active messages
- Dead letter messages
- Request count
- Server errors

### 2. Application Logs
NServiceBus provides built-in logging:
- Message processing times
- Retry attempts
- Error details

### 3. Common Issues
- **Connection errors**: Check connection string format
- **Deserialization errors**: Ensure message contracts match
- **Timeout errors**: Check network connectivity and Service Bus health

## Scaling Considerations

### 1. Horizontal Scaling
- Multiple instances can process messages concurrently
- NServiceBus handles message distribution automatically

### 2. Message Partitioning
- Use session-based partitioning for ordered processing
- Consider message size limits (256KB for Standard tier)

### 3. Performance Tuning
- Adjust concurrent message processing limits
- Configure appropriate timeout values
- Use batch processing for bulk operations

## Additional Features

### 1. Message Scheduling
```csharp
// Schedule message for future delivery
await context.Defer(TimeSpan.FromHours(1), new CommentReminderMessage());
```

### 2. Saga Pattern
For complex business processes spanning multiple services:

```csharp
public class CommentProcessingSaga : Saga<CommentProcessingSagaData>
{
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CommentProcessingSagaData> mapper)
    {
        mapper.ConfigureMapping<CommentCreatedEvent>(message => message.CommentId)
              .ToSaga(sagaData => sagaData.CommentId);
    }
}
```

### 3. Request/Response Pattern
For synchronous-like communication:

```csharp
// Send request
var response = await messageSession.Request<UserValidationResponse>(new ValidateUserRequest { UserId = 123 });
```

## Migration from HTTP to Service Bus

To migrate existing HTTP-based communication:

1. **Identify async operations**: Operations that don't need immediate response
2. **Create message contracts**: Define events and commands
3. **Implement gradually**: Start with non-critical operations
4. **Maintain backward compatibility**: Keep HTTP endpoints during transition
5. **Monitor performance**: Compare response times and error rates

## Conclusion

Azure Service Bus with NServiceBus provides a robust, scalable solution for microservices communication. It ensures reliable message delivery, supports complex messaging patterns, and integrates well with .NET applications.

For production deployment, consider:
- Using Azure Key Vault for connection strings
- Setting up proper monitoring and alerting
- Implementing proper error handling and retry policies
- Planning for disaster recovery scenarios 