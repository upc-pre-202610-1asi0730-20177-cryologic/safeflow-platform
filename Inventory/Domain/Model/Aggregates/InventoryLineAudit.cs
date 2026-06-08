using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Inventory.Domain.Model.Aggregates;

public partial class InventoryLine : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
