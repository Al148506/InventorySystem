using System;
using System.Collections.Generic;

namespace InventorySystem.Models;

public partial class UserRol
{
    public int IdRol { get; set; }

    public string? RolName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
