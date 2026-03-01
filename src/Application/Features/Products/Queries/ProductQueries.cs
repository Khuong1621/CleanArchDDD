using Application.Common;
using Application.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries;

// ============================================================
// GET ALL PRODUCTS (PAGED)
// ============================================================
public record GetProductsQuery(int Page = 1, int PageSize = 10, string? Search = null) 
    : IRequest<Result<PagedResult<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.Products.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);

        var dtos = items.Select(p => new ProductDto
        {
            Id = p.Id, Name = p.Name, Description = p.Description,
            Price = p.Price.Amount, Currency = p.Price.Currency,
            StockQuantity = p.StockQuantity, Status = p.Status.ToString(),
            Category = p.Category, SKU = p.SKU,
            CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
        });

        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>
        {
            Items = dtos, TotalCount = total,
            Page = request.Page, PageSize = request.PageSize
        });
    }
}

// ============================================================
// GET PRODUCT BY ID
// ============================================================
public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null) return Result<ProductDto>.Failure("Product not found.");

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
