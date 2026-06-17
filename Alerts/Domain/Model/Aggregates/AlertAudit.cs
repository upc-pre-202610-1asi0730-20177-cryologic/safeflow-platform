using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Alerts.Domain.Model.Aggregates;

public partial class Alert : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
