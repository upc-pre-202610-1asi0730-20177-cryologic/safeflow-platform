namespace SafeFlow.API.Inventory.Application.Errors;

public enum InventoryCommandError
{
    ProductNotFound,
    LineNotFound,
    DuplicateProductCode,
    DuplicateLineCode,
    UnexpectedError
}
