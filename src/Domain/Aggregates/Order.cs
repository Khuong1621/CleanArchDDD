using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Aggregates;

public class Order : BaseEntity
{
    public string CustomerEmail { get; private set; } = null!;
    public string CustomerName { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public string? Notes { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(string customerEmail, string customerName, Address shippingAddress, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(customerEmail)) throw new DomainException("Customer email is required.");

        var order = new Order
        {
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            TotalAmount = Money.Create(0, "USD"),
            CreatedBy = createdBy
        };

        return order;
    }

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending) throw new DomainException("Cannot modify a non-pending order.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
        }

        RecalculateTotal();
    }

    public void Confirm(string updatedBy)
    {
        if (Status != OrderStatus.Pending) throw new DomainException("Only pending orders can be confirmed.");
        if (!_items.Any()) throw new DomainException("Cannot confirm an empty order.");

        Status = OrderStatus.Confirmed;
        SetUpdated(updatedBy);
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerEmail, TotalAmount));
    }

    public void Cancel(string reason, string updatedBy)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new DomainException("Order cannot be cancelled in its current state.");

        Status = OrderStatus.Cancelled;
        Notes = reason;
        SetUpdated(updatedBy);
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void Ship(string updatedBy)
    {
        if (Status != OrderStatus.Confirmed) throw new DomainException("Only confirmed orders can be shipped.");
        Status = OrderStatus.Shipped;
        SetUpdated(updatedBy);
    }

    private void RecalculateTotal()
    {
        var total = _items.Sum(i => i.SubTotal.Amount);
        TotalAmount = Money.Create(total, TotalAmount.Currency);
    }
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money SubTotal => Money.Create(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    private OrderItem() { }

    public OrderItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int qty) => Quantity += qty;
}
