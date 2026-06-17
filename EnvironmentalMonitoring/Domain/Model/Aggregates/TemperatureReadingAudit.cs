using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;

public partial class TemperatureReading : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
