using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models.ViewModels;
public class RegisterViewModel
{
    public int IdUser { get; set; }
    [Required]
    public string? UserMail { get; set; }
    [Required]
    public string? UserPassword { get; set; }

    public string? UserName { get; set; }

    public DateTime? CreationDate { get; set; }
    public DateTime? LastModDate { get; set; }

    public int? IdRol { get; set; }
    public string ConfirmPassword { get; set; }

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual UserRol? IdRolNavigation { get; set; }
}
