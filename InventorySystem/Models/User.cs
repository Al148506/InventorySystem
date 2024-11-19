using System;
using System.Collections.Generic;

namespace InventorySystem.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string? Mail { get; set; }

    public string? Password { get; set; }

    public string? UserName { get; set; }

    public DateTime? CreationDate { get; set; }

    public int? IdRol { get; set; }

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual UserRol? IdRolNavigation { get; set; }
}
