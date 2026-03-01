using Application.Common;
using Application.DTOs;
using Domain.Aggregates;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Features.Products.Commands;

// ============================================================
// CREATE PRODUCT
// ============================================================
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    string Category,
    string SKU
) : IRequest<Result<ProductDto>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Products.GetBySkuAsync(request.SKU, cancellationToken);
        if (existing != null) return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists.");

        var product = Product.Create(
            request.Name, request.Description,
            Money.Create(request.Price, request.Currency),
            request.StockQuantity, request.Category, request.SKU, "system");

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(MapToDto(product));
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id, Name = p.Name, Description = p.Description,
        Price = p.Price.Amount, Currency = p.Price.Currency,
        StockQuantity = p.StockQuantity, Status = p.Status.ToString(),
        Category = p.Category, SKU = p.SKU,
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
}

// ============================================================
// UPDATE PRODUCT PRICE
// ============================================================
public record UpdateProductPriceCommand(Guid ProductId, decimal NewPrice, string Currency) : IRequest<Result<ProductDto>>;

public class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewPrice).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductPriceCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<ProductDto>> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null) return Result<ProductDto>.Failure("Product not found.");

        product.UpdatePrice(Money.Create(request.NewPrice, request.Currency), "system");
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(new ProductDto
        {
            Id = product.Id, Name = product.Name, Description = product.Description,
            Price = product.Price.Amount, Currency = product.Price.Currency,
            StockQuantity = product.StockQuantity, Status = product.Status.ToString(),
            Category = product.Category, SKU = product.SKU,
            CreatedAt = product.CreatedAt, UpdatedAt = product.UpdatedAt
        });
    }
}

// ============================================================
// DELETE PRODUCT
// ============================================================
public record DeleteProductCommand(Guid ProductId) : IRequest<Result>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("Product not found.");

        product.Deactivate("system");
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
