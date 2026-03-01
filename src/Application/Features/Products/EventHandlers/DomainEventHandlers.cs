using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.EventHandlers;

public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger) => _logger = logger;

    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🎉 Product created: {ProductId} - {ProductName} @ {Price}",
            notification.ProductId, notification.ProductName, notification.Price);

        // Here you'd send emails, publish to message bus, update read model, etc.
        return Task.CompletedTask;
    }
}

public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedEvent>
{
    private readonly ILogger<OrderConfirmedEventHandler> _logger;

    public OrderConfirmedEventHandler(ILogger<OrderConfirmedEventHandler> logger) => _logger = logger;

    public Task Handle(OrderConfirmedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✅ Order confirmed: {OrderId} for {Email} - Total: {Total}",
            notification.OrderId, notification.CustomerEmail, notification.TotalAmount);

        // Send confirmation email, notify warehouse, etc.
        return Task.CompletedTask;
    }
}
