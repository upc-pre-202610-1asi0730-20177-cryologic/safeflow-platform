using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Logistics.Interfaces.REST.Transform;

/// <summary>
/// Assembler for transforming Logistics domain aggregates into REST API resource objects.
/// </summary>
/// <remarks>
/// Converts domain models to anonymous typed objects suitable for JSON serialization.
/// Handles multilingual name mapping via <see cref="LocalizedText"/>.
/// </remarks>
public static class LogisticsResourceAssembler
{
    /// <summary>
    /// Transforms a <see cref="LogisticsDestination"/> aggregate into a destination API resource.
    /// </summary>
    /// <param name="d">The destination domain aggregate.</param>
    /// <returns>
    /// Anonymous object with properties:
    /// <list type="bullet">
    /// <item><description><c>idDestino</c>: Unique destination code</description></item>
    /// <item><description><c>codigo</c>: Slug identifier</description></item>
    /// <item><description><c>nombre</c>: Localized destination name object</description></item>
    /// </list>
    /// </returns>
    public static object ToDestinoResource(LogisticsDestination d) => new
    {
        idDestino = d.DestinationCode,
        codigo = d.Slug,
        nombre = LocalizedText.FromRaw(d.NameJson).ToApiObject()
    };

    /// <summary>
    /// Transforms a <see cref="LogisticsDriver"/> aggregate into a driver API resource.
    /// </summary>
    /// <param name="c">The driver domain aggregate.</param>
    /// <returns>
    /// Anonymous object with properties:
    /// <list type="bullet">
    /// <item><description><c>idChofer</c>: Unique driver code</description></item>
    /// <item><description><c>idTransportista</c>: Associated carrier code</description></item>
    /// <item><description><c>codigo</c>: Employee/reference code</description></item>
    /// <item><description><c>nombre</c>: Localized driver name object</description></item>
    /// <item><description><c>licencia</c>: Driver license identifier</description></item>
    /// <item><description><c>contacto</c>: Contact information</description></item>
    /// <item><description><c>rol</c>: Role or position designation</description></item>
    /// </list>
    /// </returns>
    public static object ToChoferResource(LogisticsDriver c) => new
    {
        idChofer = c.DriverCode,
        idTransportista = c.CarrierCode,
        codigo = c.EmployeeCode,
        nombre = LocalizedText.FromRaw(c.NameJson).ToApiObject(),
        licencia = c.License,
        contacto = c.Contact,
        rol = c.Role
    };
}