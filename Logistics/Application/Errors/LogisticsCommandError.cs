namespace SafeFlow.API.Logistics.Application.Errors;

/// <summary>
/// Defines domain and application-level business rule violation errors 
/// occurring during the execution of logistics command services.
/// </summary>
public enum LogisticsCommandError
{
    /// <summary>Indicates required payload attributes are missing, malformed, or empty.</summary>
    InvalidFields,

    /// <summary>Indicates a resource identifier, code, or unique slug already exists in the system.</summary>
    CodigoExists,

    /// <summary>Indicates the targeted tracking aggregate or entity could not be found.</summary>
    NotFound,

    /// <summary>Indicates an operation failed because the underlying carrier/fleet configuration is missing.</summary>
    FleetNotConfigured,

    /// <summary>Indicates the specified inventory stock line item does not exist in records.</summary>
    InventoryItemNotFound,

    /// <summary>Indicates a personnel assignment mismatch (e.g., assigning an operator to a driver role).</summary>
    InvalidDriver,

    /// <summary>Indicates the requested target route or handling personnel mapping is invalid.</summary>
    InvalidDestinationOrDriver,

    /// <summary>Indicates requested dispatch quantities are negative, zero, or exceed available line stock.</summary>
    InvalidQty,

    /// <summary>Indicates an unsupported shipment status transition attempt.</summary>
    InvalidStatus,

    /// <summary>Indicates a deletion block due to active referential integrity constraints (resource is in use).</summary>
    InUse,

    /// <summary>Indicates a structural or unhandled system failure during execution.</summary>
    UnexpectedError
}