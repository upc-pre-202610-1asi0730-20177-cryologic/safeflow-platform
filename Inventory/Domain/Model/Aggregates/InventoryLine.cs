using SafeFlow.API.Inventory.Domain.Model.Commands;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Aggregates;

public partial class InventoryLine
{
    protected InventoryLine() { LineCode = null!; Location = null!; Product = null!; }

    public InventoryLine(CreateStockLineCommand command, Product product)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(product);
        LineCode = command.LineCode;
        Product = product;
        ProductId = product.Id;
        Quantity = command.Quantity;
        Location = command.Location;
        EntryDate = command.EntryDate;
    }

    public int Id { get; private set; }
    public InventoryLineCode LineCode { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; }
    public int Quantity { get; private set; }
    public string Location { get; private set; }
    public DateOnly EntryDate { get; private set; }

    public void ApplyUpdate(UpdateInventoryItemCommand command)
    {
        if (command.Quantity.HasValue) Quantity = Math.Max(0, command.Quantity.Value);
        if (!string.IsNullOrWhiteSpace(command.Location)) Location = command.Location.Trim();
    }
}
