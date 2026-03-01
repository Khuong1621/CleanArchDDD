namespace Application.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public int StockQuantity { get; init; }
    public string Status { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string SKU { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record OrderDto
{
    public Guid Id { get; init; }
    public string CustomerEmail { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = null!;
    public string ShippingAddress { get; init; } = null!;
    public List<OrderItemDto> Items { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal SubTotal { get; init; }
}
