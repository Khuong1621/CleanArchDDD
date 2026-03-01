using Domain.ValueObjects;

namespace Domain.Events;

public record ProductCreatedEvent(Guid ProductId, string ProductName, Money Price) : BaseDomainEvent;
public record ProductPriceChangedEvent(Guid ProductId, Money OldPrice, Money NewPrice) : BaseDomainEvent;
public record ProductDeactivatedEvent(Guid ProductId) : BaseDomainEvent;

public record OrderConfirmedEvent(Guid OrderId, string CustomerEmail, Money TotalAmount) : BaseDomainEvent;
public record OrderCancelledEvent(Guid OrderId, string Reason) : BaseDomainEvent;
