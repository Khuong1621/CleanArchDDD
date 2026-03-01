using Application.Common;
using Application.DTOs;
using Domain.Aggregates;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Features.Orders.Commands;

// ============================================================
// CREATE ORDER
// ============================================================
public record CreateOrderCommand(
    string CustomerEmail,
    string CustomerName,
    string Street, string City, string State, string Country, string ZipCode,
    List<CreateOrderItemDto> Items
) : IRequest<Result<OrderDto>>;

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var address = Address.Create(request.Street, request.City, request.State, request.Country, request.ZipCode);
            var order = Order.Create(request.CustomerEmail, request.CustomerName, address, "system");

            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null) return Result<OrderDto>.Failure($"Product {item.ProductId} not found.");

                product.DeductStock(item.Quantity, "system");
                order.AddItem(product.Id, product.Name, product.Price, item.Quantity);
                _unitOfWork.Products.Update(product);
            }

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<OrderDto>.Success(MapToDto(order));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id, CustomerEmail = o.CustomerEmail, CustomerName = o.CustomerName,
        Status = o.Status.ToString(), TotalAmount = o.TotalAmount.Amount,
        Currency = o.TotalAmount.Currency,
        ShippingAddress = o.ShippingAddress.ToString(),
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId, ProductName = i.ProductName,
            UnitPrice = i.UnitPrice.Amount, Quantity = i.Quantity,
            SubTotal = i.SubTotal.Amount
        }).ToList(),
        CreatedAt = o.CreatedAt
    };
}

// ============================================================
// CONFIRM ORDER
// ============================================================
public record ConfirmOrderCommand(Guid OrderId) : IRequest<Result<OrderDto>>;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<OrderDto>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order == null) return Result<OrderDto>.Failure("Order not found.");

        order.Confirm("system");
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderDto>.Success(new OrderDto
        {
            Id = order.Id, CustomerEmail = order.CustomerEmail, CustomerName = order.CustomerName,
            Status = order.Status.ToString(), TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            ShippingAddress = order.ShippingAddress.ToString(),
            CreatedAt = order.CreatedAt
        });
    }
}
