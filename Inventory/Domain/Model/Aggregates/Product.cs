using SafeFlow.API.Inventory.Domain.Model.Commands;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Aggregates;

public partial class Product
{
    protected Product() { ProductCode = null!; Name = null!; Category = null!; Status = null!; Batch = null!; }

    public Product(CreateProductCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        ProductCode = command.ProductCode;
        Name = command.Name;
        Category = command.Category;
        TemperatureMin = command.TemperatureMin;
        TemperatureMax = command.TemperatureMax;
        ExpiryDate = command.ExpiryDate;
        Batch = command.Batch;
        Status = command.Status;
    }

    public int Id { get; private set; }
    public ProductCode ProductCode { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public decimal TemperatureMin { get; private set; }
    public decimal TemperatureMax { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string Batch { get; private set; }
    public string Status { get; private set; }

    public ICollection<InventoryLine> Lines { get; private set; } = new List<InventoryLine>();

    public void ApplyUpdate(UpdateInventoryItemCommand command)
    {
        if (!string.IsNullOrWhiteSpace(command.Name)) Name = command.Name.Trim();
        if (!string.IsNullOrWhiteSpace(command.Category)) Category = command.Category.Trim();
        if (!string.IsNullOrWhiteSpace(command.Status)) Status = command.Status.Trim();
        if (command.TemperatureMin.HasValue) TemperatureMin = command.TemperatureMin.Value;
        if (command.TemperatureMax.HasValue) TemperatureMax = command.TemperatureMax.Value;
        if (!string.IsNullOrWhiteSpace(command.Batch)) Batch = command.Batch.Trim();
        if (command.ExpiryDate.HasValue) ExpiryDate = command.ExpiryDate;
    }
}
