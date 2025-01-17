using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models.ViewModels;
public class RegisterViewModel
{
    public int IdUser { get; set; }
    [Required]
    public string? UserMail { get; set; }
    [Required]
    public string? UserPassword { get; set; }
    [Required]
    public string? UserName { get; set; }

    public DateTime? CreationDate { get; set; }
    public DateTime? LastModDate { get; set; }
    [Required]
    [Range(1, 3, ErrorMessage = "Por favor, selecciona un rol válido.")]
    public int? IdRol { get; set; }
    public string? ConfirmPassword { get; set; }

    //public static ValidationResult ValidatePassword(RegisterViewModel register)
    //{
    //    ValidationResult result = null;

    //    if (register.UserPassword != register.ConfirmPassword)
    //    {
    //        result = new ValidationResult("The passwords do not match.");
    //    }
    //    return result;
    //}
    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual UserRol? IdRolNavigation { get; set; }
}
