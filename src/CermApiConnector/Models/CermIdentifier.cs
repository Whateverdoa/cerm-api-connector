using System;
using System.Collections.Generic;

namespace CermApiConnector.Models;

public enum CermIdentifierType
{
    Address,
    Product,
    QuoteCalculation,
    SalesOrder
}

public enum CermIdentifierStatus
{
    Pending,
    Processing,
    Available,
    Patched,
    Error
}

public class CermIdentifier
{
    public int Id { get; set; }
    public int OrderHeaderId { get; set; }
    public CermIdentifierType IdentifierType { get; set; }
    public string? CermIdValue { get; set; }
    public CermIdentifierStatus Status { get; set; }
    public string? Details { get; set; } // For error messages or other relevant info
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}