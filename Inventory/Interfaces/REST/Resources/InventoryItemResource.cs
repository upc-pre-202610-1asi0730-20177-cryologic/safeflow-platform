namespace SafeFlow.API.Inventory.Interfaces.REST.Resources;

public record InventoryItemResource(
    string Id,
    string? IdProducto,
    string? IdInventario,
    int Qty,
    string Status,
    string TempTone,
    object? Name,
    object? Category,
    object? TempLabel,
    object? Location,
    decimal? TemperaturaMin,
    decimal? TemperaturaMax,
    string? Lote,
    string? FechaVencimiento,
    string? FechaIngreso);
