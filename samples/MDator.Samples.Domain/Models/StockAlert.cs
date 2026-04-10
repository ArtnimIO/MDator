namespace MDator.Samples.Domain.Models;

public enum AlertType
{
    LowStock,
    OutOfStock,
    Restocked
}

public class StockAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public AlertType AlertType { get; set; }
    public int Threshold { get; set; }
    public int CurrentQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
