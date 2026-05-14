namespace Sample.State;

/// <summary>
/// Application state representing a sales order with a list of line‑item positions.
/// This is a plain CLR type – the persistence adapter only sees its serialized form.
/// </summary>
public sealed class Order
{
    /// <summary>Unique identifier for the order (business key).</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Customer name or identifier.</summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>List of line items.</summary>
    public IList<Position> Positions { get; set; } = new List<Position>();

    /// <summary>Calculated total price of the order.</summary>
    public decimal Total => Positions.Sum(p => p.Price * p.Quantity);
}

/// <summary>
/// Simple DTO that represents a line‑item in the order.
/// </summary>
public sealed class Position
{
    /// <summary>Stock‑keeping unit.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Quantity ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Unit price.</summary>
    public decimal Price { get; set; }
}