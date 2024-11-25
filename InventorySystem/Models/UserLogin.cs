using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace InventorySystem.Models;

public partial class UserLogin
{
    public int IdUser { get; set; }

    public string? UserMail { get; set; }

    public string? UserPassword { get; set; }

    public string? UserName { get; set; }

    public DateTime? CreationDate { get; set; }

    public int? IdRol { get; set; }
    public string ConfirmPassword { get; set; }

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual UserRol? IdRolNavigation { get; set; }
}
