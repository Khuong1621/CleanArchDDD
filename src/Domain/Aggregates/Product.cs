using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Aggregates;

public class Product : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public ProductStatus Status { get; private set; }
    public string Category { get; private set; } = null!;
    public string SKU { get; private set; } = null!;

    private Product() { } // EF Core

    public static Product Create(string name, string description, Money price, int stockQuantity, string category, string sku, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Product name cannot be empty.");
        if (price.Amount <= 0) throw new DomainException("Product price must be greater than zero.");
        if (stockQuantity < 0) throw new DomainException("Stock quantity cannot be negative.");

        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            SKU = sku,
            Status = ProductStatus.Active,
            CreatedBy = createdBy
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, name, price));
        return product;
    }

    public void UpdatePrice(Money newPrice, string updatedBy)
    {
        if (newPrice.Amount <= 0) throw new DomainException("Price must be greater than zero.");
        var oldPrice = Price;
        Price = newPrice;
        SetUpdated(updatedBy);
        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void AddStock(int quantity, string updatedBy)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        StockQuantity += quantity;
        SetUpdated(updatedBy);
    }

    public void DeductStock(int quantity, string updatedBy)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        if (StockQuantity < quantity) throw new DomainException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}.");
        StockQuantity -= quantity;
        SetUpdated(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        Status = ProductStatus.Inactive;
        SetUpdated(updatedBy);
        AddDomainEvent(new ProductDeactivatedEvent(Id));
    }
}
