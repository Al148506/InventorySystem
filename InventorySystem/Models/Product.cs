using System;
using System.Collections.Generic;

namespace InventorySystem.Models;

public partial class Product
{
    public int IdProd { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public int? Quantity { get; set; }

    public string? State { get; set; }

    public int? IdCategory { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? LastModDate { get; set; }

    public string? ImageRoot { get; set; }

    public int? IdLocation { get; set; }

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual Category? Category { get; set; }

    public virtual Location? Location { get; set; }
}
