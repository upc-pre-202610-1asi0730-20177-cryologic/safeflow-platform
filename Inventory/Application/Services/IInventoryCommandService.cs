using SafeFlow.API.Inventory.Application.Errors;
using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.Commands;
using SafeFlow.API.Shared.Application.Patterns;

namespace SafeFlow.API.Inventory.Application.Services;

public interface IInventoryCommandService
{
    Task<Result<InventoryLine, InventoryCommandError>> Handle(
        CreateProductCommand command, CancellationToken ct = default);

    Task<Result<InventoryLine, InventoryCommandError>> Handle(
        CreateStockLineCommand command, CancellationToken ct = default);

    Task<Result<InventoryLine, InventoryCommandError>> Handle(
        UpdateInventoryItemCommand command, CancellationToken ct = default);

    Task<Result<bool, InventoryCommandError>> Handle(
        DeleteInventoryLineCommand command, CancellationToken ct = default);
}
