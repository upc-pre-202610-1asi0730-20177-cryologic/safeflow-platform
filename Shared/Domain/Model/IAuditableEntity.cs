namespace SafeFlow.API.Shared.Domain.Model;

public interface IAuditableEntity
{
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}
