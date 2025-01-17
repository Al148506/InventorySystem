using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        public int IdUser { get; set; }
        [Required]
        public string? NewPassword { get; set; }
        [Required]
        public string? ConfirmPassword { get; set; }

    }
}
