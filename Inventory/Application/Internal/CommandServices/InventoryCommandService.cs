using SafeFlow.API.Inventory.Application.Errors;
using SafeFlow.API.Inventory.Application.Services;
using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.Commands;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Shared.Application.Patterns;
using SafeFlow.API.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Inventory.Application.Internal.CommandServices;

public class InventoryCommandService(
    IProductRepository productRepository,
    IInventoryLineRepository inventoryLineRepository,
    IUnitOfWork unitOfWork,
    ILogger<InventoryCommandService> logger) : IInventoryCommandService
{
    public async Task<Result<InventoryLine, InventoryCommandError>> Handle(
        CreateProductCommand command, CancellationToken ct = default)
    {
        var existing = await productRepository.FindByProductCodeAsync(command.ProductCode, ct);
        if (existing != null)
            return new Result<InventoryLine, InventoryCommandError>.Failure(
                InventoryCommandError.DuplicateProductCode);

        try
        {
            var product = new Product(command);
            var line = new InventoryLine(
                new CreateStockLineCommand(
                    command.ProductCode,
                    command.LineCode,
                    command.InitialQuantity,
                    command.Location,
                    command.EntryDate),
                product);
            product.Lines.Add(line);
            await productRepository.AddAsync(product, ct);
            await unitOfWork.CompleteAsync(ct);

            var loaded = await inventoryLineRepository.FindByLineCodeAsync(command.LineCode, ct);
            return new Result<InventoryLine, InventoryCommandError>.Success(loaded!);
        }
        catch (DbUpdateException ex) when (IsDuplicateKey(ex))
        {
            logger.LogWarning(ex, "Duplicate product or line");
            return new Result<InventoryLine, InventoryCommandError>.Failure(
                InventoryCommandError.DuplicateProductCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Create product failed");
            return new Result<InventoryLine, InventoryCommandError>.Failure(
                InventoryCommandError.UnexpectedError);
        }
    }

    public async Task<Result<InventoryLine, InventoryCommandError>> Handle(
        CreateStockLineCommand command, CancellationToken ct = default)
    {
        var product = await productRepository.FindByProductCodeAsync(command.ProductCode, ct);
        if (product == null)
            return new Result<InventoryLine, InventoryCommandError>.Failure(InventoryCommandError.ProductNotFound);

        var duplicate = await inventoryLineRepository.FindByLineCodeAsync(command.LineCode, ct);
        if (duplicate != null)
            return new Result<InventoryLine, InventoryCommandError>.Failure(InventoryCommandError.DuplicateLineCode);

        try
        {
            var line = new InventoryLine(command, product);
            await inventoryLineRepository.AddAsync(line, ct);
            await unitOfWork.CompleteAsync(ct);
            var loaded = await inventoryLineRepository.FindByLineCodeAsync(command.LineCode, ct);
            return new Result<InventoryLine, InventoryCommandError>.Success(loaded!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Create stock line failed");
            return new Result<InventoryLine, InventoryCommandError>.Failure(InventoryCommandError.UnexpectedError);
        }
    }

    public async Task<Result<InventoryLine, InventoryCommandError>> Handle(
        UpdateInventoryItemCommand command, CancellationToken ct = default)
    {
        var line = await inventoryLineRepository.FindByLineCodeAsync(command.LineCode, ct);
        if (line == null)
            return new Result<InventoryLine, InventoryCommandError>.Failure(InventoryCommandError.LineNotFound);

        line.ApplyUpdate(command);
        line.Product.ApplyUpdate(command);
        inventoryLineRepository.Update(line);
        productRepository.Update(line.Product);

        try
        {
            await unitOfWork.CompleteAsync(ct);
            var loaded = await inventoryLineRepository.FindByLineCodeAsync(command.LineCode, ct);
            return new Result<InventoryLine, InventoryCommandError>.Success(loaded!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Update inventory item failed");
            return new Result<InventoryLine, InventoryCommandError>.Failure(InventoryCommandError.UnexpectedError);
        }
    }

    public async Task<Result<bool, InventoryCommandError>> Handle(
        DeleteInventoryLineCommand command, CancellationToken ct = default)
    {
        var line = await inventoryLineRepository.FindByLineCodeAsync(command.LineCode, ct);
        if (line == null)
            return new Result<bool, InventoryCommandError>.Failure(InventoryCommandError.LineNotFound);

        var productId = line.ProductId;
        inventoryLineRepository.Remove(line);

        try
        {
            await unitOfWork.CompleteAsync(ct);
            var remaining = await inventoryLineRepository.CountByProductIdAsync(productId, ct);
            if (remaining == 0)
            {
                var product = await productRepository.FindByIdAsync(productId, ct);
                if (product != null)
                {
                    productRepository.Remove(product);
                    await unitOfWork.CompleteAsync(ct);
                }
            }

            return new Result<bool, InventoryCommandError>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Delete inventory line failed");
            return new Result<bool, InventoryCommandError>.Failure(InventoryCommandError.UnexpectedError);
        }
    }

    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
        {
            if (!string.Equals(current.GetType().Name, "MySqlException", StringComparison.Ordinal)) continue;
            var numberProperty = current.GetType().GetProperty("Number");
            if (numberProperty?.PropertyType == typeof(int) &&
                numberProperty.GetValue(current) is int code && code == 1062) return true;
        }

        return false;
    }
}
