using Application.DTOs;
using Application.Features.Orders.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Orders management API
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a new order</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(ConfirmOrder), new { id = result.Value!.Id, version = "1" }, ApiResponse<OrderDto>.Succeed(result.Value))
            : BadRequest(ApiResponse<string>.Fail(result.Error!));
    }

    /// <summary>Confirm a pending order</summary>
    /// <param name="id">Order ID</param>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmOrder(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmOrderCommand(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<OrderDto>.Succeed(result.Value!))
            : BadRequest(ApiResponse<string>.Fail(result.Error!));
    }
}
