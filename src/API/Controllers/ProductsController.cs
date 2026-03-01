using Application.Common;
using Application.DTOs;
using Application.Features.Products.Commands;
using Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Products management API
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get paginated list of products</summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="search">Optional search term</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProductsQuery(page, pageSize, search), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<PagedResult<ProductDto>>.Succeed(result.Value!))
            : BadRequest(ApiResponse<string>.Fail(result.Error!));
    }

    /// <summary>Get product by ID</summary>
    /// <param name="id">Product ID</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<ProductDto>.Succeed(result.Value!))
            : NotFound(ApiResponse<string>.Fail(result.Error!));
    }

    /// <summary>Create a new product</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProduct), new { id = result.Value!.Id, version = "1" }, ApiResponse<ProductDto>.Succeed(result.Value))
            : BadRequest(ApiResponse<string>.Fail(result.Error!));
    }

    /// <summary>Update product price</summary>
    /// <param name="id">Product ID</param>
    [HttpPatch("{id:guid}/price")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest req, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductPriceCommand(id, req.NewPrice, req.Currency), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<ProductDto>.Succeed(result.Value!))
            : BadRequest(ApiResponse<string>.Fail(result.Error!));
    }

    /// <summary>Deactivate a product (soft delete)</summary>
    /// <param name="id">Product ID</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(ApiResponse<string>.Fail(result.Error!));
    }
}

public record UpdatePriceRequest(decimal NewPrice, string Currency);

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Succeed(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}
