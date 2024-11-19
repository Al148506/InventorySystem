using System;
using System.Collections.Generic;

namespace InventorySystem.Models;

public partial class Location
{
    public int IdLocation { get; set; }

    public string? LocationName { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
