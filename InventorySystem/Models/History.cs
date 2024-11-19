using System;
using System.Collections.Generic;

namespace InventorySystem.Models;

public partial class History
{
    public int IdHistory { get; set; }

    public int? IdProd { get; set; }

    public int? IdUser { get; set; }

    public DateTime? DateMod { get; set; }

    public string? ChangeType { get; set; }

    public string? PreviousValue { get; set; }

    public string? CurrentValue { get; set; }

    public virtual Product? IdProdNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
